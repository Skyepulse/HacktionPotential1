import time
import numpy as np
import pylsl
from scipy.signal import butter, filtfilt

def bandpass_filter(X, fs, low=0.5, high=30.0, order=4):
    nyq = 0.5 * fs
    high = min(high, nyq * 0.99)
    b, a = butter(order, [low/nyq, high/nyq], btype="band")
    return filtfilt(b, a, X, axis=0)

def connect_lsl(stream_name="EEGStream", timeout=5):
    streams = pylsl.resolve_byprop("name", 'Explore_AACG_ExG', timeout=timeout)
    # streams = pylsl.resolve_streams()
    if not streams:
        raise RuntimeError(f"Pas de stream {stream_name}")
    info = streams[0]
    fs = float(info.nominal_srate()) if info.nominal_srate() and info.nominal_srate() > 0 else 250.0
    n_ch = int(info.channel_count())
    inlet = pylsl.StreamInlet(info, max_buflen=60)  # buffer interne LSL
    print("Connecté:", info.name(), "| ch:", n_ch, "| fs:", fs)
    return inlet, fs, n_ch

def read_seconds(inlet, fs, seconds):
    """Lit ~seconds de données et renvoie (N, n_ch)."""
    n_target = int(round(fs * seconds))
    data = []
    while len(data) < n_target:
        chunk, _ = inlet.pull_chunk(timeout=1.0, max_samples=n_target - len(data))
        if chunk:
            data.extend(chunk)
    return np.asarray(data, dtype=np.float32)

def collect_mains(inlet, fs, n_trials_per_class=30, rest_s=2.0, mi_s=4.0, out_file="mi_dataset.npz", seed=0):
    """
    y: 0 = LEFT, 1 = RIGHT
    """
    rng = np.random.default_rng(seed)

    #aléatoire
    labels = np.array([0]*n_trials_per_class + [1]*n_trials_per_class, dtype=np.int64)
    rng.shuffle(labels)

    X_trials = []
    y_trials = []
    r_repos = []

    print("\nProtocole:")
    print(f"- Repos: {rest_s}s")
    print(f"- Imagerie motrice: {mi_s}s")
    print("Labels: 0=LEFT, 1=RIGHT\n")

    for k, y in enumerate(labels, start=1):
        cue = "MAIN GAUCHE: AVANCER" if y == 0 else "MAIN DROITE : BOUTON"

        print(f"Trial {k}/{len(labels)} — Repos ({rest_s}s). ")
        R = read_seconds(inlet, fs, rest_s)  # on jette le repos (NON)

        print(f"Trial {k}/{len(labels)} — IMAGINE: {cue} ({mi_s}s) ...")
        X = read_seconds(inlet, fs, mi_s)  # (N, n_ch)

        # filtrage
        # Xf = bandpass_filter(X, fs, 0.5, 30.0)
        Xf = X

        X_trials.append(Xf)
        y_trials.append(y)
        r_repos.append(R)

        print(f"  -> enregistré: {Xf.shape}\n")
    r_repos = np.stack(r_repos)
    X_trials = np.stack(X_trials)          # (n_trials, n_samples, n_ch)
    y_trials = np.array(y_trials, dtype=np.int64)

    np.savez(out_file, X=X_trials, y=y_trials, z=r_repos, fs=fs)
    print("Sauvé:", out_file, "| X:", X_trials.shape, "| y:", y_trials.shape, "| z:", r_repos.shape)


if __name__ == "__main__":
    inlet, fs, n_ch = connect_lsl("EEGStream") # Enregistrer repos
    collect_mains(inlet, fs, n_trials_per_class=30, rest_s=6.0, mi_s=6.0, out_file="mi_dataset.npz") #J'ai mis 5 pour que ce soit rapide mais il faudra remttre les 30 si besoin