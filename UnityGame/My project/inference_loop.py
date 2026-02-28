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

from collect_realtime import connect_lsl, iter_sliding_windows

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
    #name_connect = "EEGStream"  # TODO A changer pour le vrai nom : Explore_AACG_ExG

    while True:
        inlet, fs, n_ch = connect_lsl(name_connect)

        for W in iter_sliding_windows(inlet, fs, n_ch, win_s=6.0, hop_s=0.5):

            prediction = random.randint(0,3) #TODO Rajouter l'inférence(W)

            if prediction == 0 :
                print("Pas de commandes")
            elif prediction == 1 : # TODO : Mettre un cd pour l'overlap ?
                print("Commande 1: tourner")
            else :
                print("Commande 2 : Avancer")

            outlet.push_sample([int(prediction)])
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
