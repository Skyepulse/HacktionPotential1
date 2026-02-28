import os
import time
import numpy as np
import pylsl

name = "Explore_AACG_ExG" # TODO A changer pour le vrai nom : Explore_AACG_ExG
#name = "EEGStream" # TODO A changer pour le vrai nom : Explore_AACG_ExG
def connect_lsl(stream_name=name, timeout=5): # TODO A changer pour le vrai nom : Explore_AACG_ExG
    streams = pylsl.resolve_byprop("name", name, timeout=timeout)
    if not streams:
        raise RuntimeError(f"Pas de stream {stream_name}")

    info = streams[0]
    fs = float(info.nominal_srate()) if info.nominal_srate() and info.nominal_srate() > 0 else 250.0
    n_ch = int(info.channel_count())

    inlet = pylsl.StreamInlet(info, max_buflen=60)
    print("Connecté:", info.name(), "| ch:", n_ch, "| fs:", fs)
    return inlet, fs, n_ch


def init_npz(npz_path, fs, n_ch, temps=6.0):
    n_samp = int(round(temps * fs))
    X = np.empty((0, n_samp, n_ch), dtype=np.float32)  # (n_trials, n_samples, n_ch)
    y = np.empty((0,), dtype=np.int64)                 # (n_trials,)

    np.savez(npz_path, X=X, y=y, fs=np.array(fs), n_ch=np.array(n_ch), temps=np.array(temps))
    print(f"Fichier initialisé: {npz_path} | X={X.shape} y={y.shape}")


def _flush_inlet(inlet, seconds=0.5):
    t_end = time.time() + seconds
    dropped = 0
    while time.time() < t_end:
        chunk, _ = inlet.pull_chunk(timeout=0.0, max_samples=1024)
        if not chunk:
            break
        dropped += len(chunk)
    return dropped


def _record_fixed_window(inlet, fs, n_ch, temps=8.0):
    n_target = int(round(temps * fs))
    data = []

    while len(data) < n_target:
        need = n_target - len(data)
        chunk, _ = inlet.pull_chunk(timeout=1.0, max_samples=need)
        if chunk:
            data.extend(chunk)

    X_trial = np.asarray(data, dtype=np.float32)

    # Sécurité si le stream a plus de canaux que prévu
    if X_trial.ndim == 2 and X_trial.shape[1] > n_ch:
        X_trial = X_trial[:, :n_ch]

    if X_trial.shape != (n_target, n_ch):
        raise RuntimeError(f"Shape inattendue: {X_trial.shape}, attendu {(n_target, n_ch)}")

    return X_trial


def _record_trimmed_window(inlet, fs, n_ch, record_s=8.0, skip_s=2.0):
    X_raw = _record_fixed_window(inlet, fs, n_ch, temps=record_s)

    n_record = X_raw.shape[0]
    n_skip = int(round(skip_s * fs))
    if n_skip >= n_record:
        raise RuntimeError(f"skip_s trop grand: {skip_s}s (n_skip={n_skip}) >= n_record={n_record}")

    X_keep = X_raw[n_skip:, :]  # il reste (record_s - skip_s) secondes
    return X_keep


def collect_main(inlet, fs, n_ch, main, record_s=8.0, skip_s=2.0, npz_path="dataset_calibration.npz"):
    """
    Enregistre record_s secondes, supprime les skip_s premières, sauvegarde le reste.
    Ex: record_s=8, skip_s=2 => sauvegarde 6 secondes.
    """
    label = int(main)
    keep_s = record_s - skip_s
    if keep_s <= 0:
        raise ValueError("record_s doit être > skip_s")

    # Si le fichier n'existe pas, on l'initialise avec la durée finale (6s)
    if not os.path.exists(npz_path):
        init_npz(npz_path, fs, n_ch, temps=keep_s)

    _flush_inlet(inlet, seconds=0.5)

    X_trial = _record_trimmed_window(inlet, fs, n_ch, record_s=record_s, skip_s=skip_s)

    # Vérif shape finale
    n_keep = int(round(keep_s * fs))
    if X_trial.shape != (n_keep, n_ch):
        raise RuntimeError(f"Shape finale inattendue: {X_trial.shape}, attendu {(n_keep, n_ch)}")

    with np.load(npz_path, allow_pickle=False) as d:
        X_old = d["X"]
        y_old = d["y"]

    X_new = np.concatenate([X_old, X_trial[None, :, :]], axis=0)
    y_new = np.concatenate([y_old, np.array([label], dtype=np.int64)], axis=0)

    np.savez(npz_path, X=X_new, y=y_new, fs=np.array(fs), n_ch=np.array(n_ch), temps=np.array(keep_s))
    print(f"Ajout OK: label={label} | trial={X_trial.shape} | dataset={X_new.shape}")
    return X_trial, label


# if __name__ == "__main__":
#     inlet, fs, n_ch = connect_lsl(stream_name="EEGStream") #a changer
#     for i in range(10):
#         main = i % 2
#         collect_main(inlet, fs, n_ch, main=main, npz_path="dataset_calibration.npz")
