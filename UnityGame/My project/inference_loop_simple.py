###########################################
# SCRIPT PYTHON -> UNITY.
# C'EST LE SCRIPT A MODIFIER QUI ENVERRA 
# LES INFERENCES DE MIREPNET A UNITY.
###########################################
import random
# to get python prints in Unity console
import sys
sys.stdout.flush()
from pylsl import StreamInfo, StreamOutlet
import time
import numpy as np
from scipy.signal import welch
import matplotlib.pyplot as plt
from sklearn.preprocessing import StandardScaler
from sklearn.discriminant_analysis import LinearDiscriminantAnalysis
from sklearn.metrics import confusion_matrix, classification_report, accuracy_score

from collect_realtime import connect_lsl, iter_sliding_windows

#==========================#
def lda_predict_from_npz(data, model_dict):
    """
    Predicts Left (0), Right (1), Neutral (2)
    from new .npz file.
    """

    features = lda_extract_features_from_npz(
        data,
        fs=model_dict["fs"]
    )

    features_norm = model_dict["scaler"].transform(features)
    LD1 = model_dict["lda"].transform(features_norm).flatten()

    midpoint = model_dict["midpoint"]
    margin = model_dict["margin"]

    y_pred = np.zeros(len(LD1))

    for i, val in enumerate(LD1):
        if val < midpoint - margin:
            y_pred[i] = 0
        elif val > midpoint + margin:
            y_pred[i] = 1
        else:
            y_pred[i] = 2

    return y_pred.astype(int)

#==========================#

def calibrate_from_npz(data,
                       fs=250,
                       margin_factor=0.5,
                       show_kpi=False,
                       show_lda=False):
    """
    Calibrates LDA-based 3-class predictor (Left/Right/Neutral)
    using only Left and Right calibration data.
    """

    # --- Extract features ---

    X = data["X"]         # (samples, time, channels)
    y = data["y"]

    features = lda_extract_features_from_npz(data = X, fs = fs)

    # --- Normalize ---
    scaler = StandardScaler()
    features_norm = scaler.fit_transform(features)

    # --- Train LDA ---
    lda = LinearDiscriminantAnalysis(n_components=1)
    lda.fit(features_norm, y)

    LD1 = lda.transform(features_norm).flatten()

    # --- Compute midpoint ---
    mean0 = LD1[y==0].mean()
    mean1 = LD1[y==1].mean()
    midpoint = (mean0 + mean1) / 2

    sigma = np.std(LD1)
    margin = margin_factor * sigma

    # --- 3-class decision ---
    y_pred = np.zeros(len(LD1))

    for i, val in enumerate(LD1):
        if val < midpoint - margin:
            y_pred[i] = 0
        elif val > midpoint + margin:
            y_pred[i] = 1
        else:
            y_pred[i] = 2

    # --- KPIs ---
    if show_kpi:
        mask_confident = y_pred != 2
        print("Confident Accuracy:",
              accuracy_score(y[mask_confident], y_pred[mask_confident]))

        print("\nConfusion Matrix (confident only):")
        print(confusion_matrix(y[mask_confident], y_pred[mask_confident]))

        print("\nClassification Report:")
        print(classification_report(y[mask_confident], y_pred[mask_confident]))

    # --- LDA Plot ---
    if show_lda:
        plt.figure()
        plt.hist(LD1[y==0], alpha=0.6, label="Left")
        plt.hist(LD1[y==1], alpha=0.6, label="Right")

        plt.axvline(midpoint, color='k', linestyle='--', label="Midpoint")
        plt.axvspan(midpoint-margin, midpoint+margin,
                    color='gray', alpha=0.2, label="Neutral Zone")

        plt.legend()
        plt.title("LDA Calibration with Neutral Zone")
        plt.show()

    model_dict = {
        "scaler": scaler,
        "lda": lda,
        "midpoint": midpoint,
        "margin": margin,
        "fs": fs
    }

    return model_dict

def lda_extract_features_from_npz(data, fs=250, band=(8,25)):
    """
    Loads .npz file and extracts lateralization features.
    
    Returns:
        features (n_samples, 3)
    """

    X = data       # (samples, time, channels)

    n_samples, _, n_channels = X.shape
    band_power = np.zeros((n_samples, n_channels))

    # Compute band power via Welch
    for s in range(n_samples):
        for ch in range(n_channels):

            freqs, psd = welch(X[s,:,ch], fs=fs, nperseg=512)
            mask = (freqs >= band[0]) & (freqs <= band[1])
            band_power[s,ch] = np.trapz(psd[mask], freqs[mask])

    # Log-power improves stability
    band_power = np.log(band_power + 1e-10)

    # Lateralization indices
    LI_34 = (band_power[:,2] - band_power[:,3]) / (band_power[:,2] + band_power[:,3])
    LI_56 = (band_power[:,4] - band_power[:,5]) / (band_power[:,4] + band_power[:,5])
    LI_78 = (band_power[:,6] - band_power[:,7]) / (band_power[:,6] + band_power[:,7])

    features = np.vstack([LI_34, LI_56, LI_78]).T

    return features

def main(): # LSL vers unity de markers 0/1 :
    # Create LSL stream info
    info = StreamInfo(
        name='InputStream',
        type='Markers',
        channel_count=1,
        nominal_srate=0,          # irregular sampling rate
        channel_format='int32',
        source_id='input_01'
    )
    # Create outlet
    outlet = StreamOutlet(info)
    print(f"LSL outlet started: {info.name()}")

    name_connect = "Explore_AACG_ExG"
    name_connect = "EEGStream"  # TODO A changer pour le vrai nom : Explore_AACG_ExG

    npz_path = "dataset_calibration.npz"
    data = np.load(npz_path)
    model = calibrate_from_npz(
        data,
        fs = data['fs'].item(),
        margin_factor=0.5,
        show_kpi=False,
        show_lda=False
    )

    while True:
        inlet, fs, n_ch = connect_lsl(name_connect)

        for W in iter_sliding_windows(inlet, fs, n_ch, win_s=6.0, hop_s=2.0):
            
            W_reshaped = W.reshape(1, W.shape[0], W.shape[1])
            prediction = lda_predict_from_npz(W_reshaped, model)
            print("prediction:", prediction)

            if prediction == 0 :
                outlet.push_sample([0])
            elif prediction == 1 :
                outlet.push_sample([1])
            else :
                print("NEUTRAL")

            # print(f"Prediction envoyée : {prediction}")
            # print("Array utilisé :", W)


# LSL vers
    # n_ch, fs = 8, 250.0
    # info = StreamInfo(
    #     name="EEG_W",
    #     type="EEG",
    #     channel_count=n_ch,
    #     nominal_srate=fs,  # régulier
    #     channel_format="float32",
    #     source_id="eeg_windowed_01",
    # )
    # outlet = StreamOutlet(info)
    # print(f"LSL outlet started: {info.name()} | ch:{n_ch} | fs:{fs}", flush=True)
    #
    #
    # name_connect = "Explore_AACG_ExG"
    # name_connect = "EEGStream" # TODO A changer pour le vrai nom : Explore_AACG_ExG
    #
    # while True:
    #    inlet, fs, n_ch = connect_lsl(name_connect)
    #    for W in iter_sliding_windows(inlet, fs, n_ch, win_s=5.0, hop_s=0.5):
    #        outlet.push_chunk(W.tolist())  # push du chunk toutes les 500ms dans stream
    #        print("Fenêtre prête:", W.shape)


if __name__ == "__main__":
    main()
