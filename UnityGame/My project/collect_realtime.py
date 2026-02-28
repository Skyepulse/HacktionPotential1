import numpy as np
import pylsl



def connect_lsl(stream_name="EEGStream", timeout=5): # TODO MODIFIER EEGStream par  Explore_AACG_ExG
    streams = pylsl.resolve_byprop("name", stream_name, timeout=timeout)
    if not streams:
        raise RuntimeError(f"Pas de stream: {stream_name}")

    info = streams[0]
    fs0 = info.nominal_srate()
    fs = float(fs0) if fs0 and fs0 > 0 else 250.0
    n_ch = int(info.channel_count())

    inlet = pylsl.StreamInlet(info, max_buflen=60)
    print("Connecté:", info.name(), "| ch:", n_ch, "| fs:", fs)
    return inlet, fs, n_ch

#lire en continu le flux EEG, construire une fenêtre glissante de 5 s,
#sortir une nouvelle fenêtre toutes les 500 ms (4,5 s de recouvrement + 0,5 s de nouvelles données)
def iter_sliding_windows(inlet, fs, n_ch, win_s=5.0, hop_s=0.5):
    win = int(round(fs*win_s)) #250Hz*5s=1250
    hop = int(round(fs*hop_s)) #250Hz*0.5s=125

    buf = np.zeros((win, n_ch), dtype=np.float32)
    filled = 0
    widx = 0 #prochaine position
    since_emit = 0 #nb reçus depuis la dernière émission

    while True:
        chunk, _ = inlet.pull_chunk(timeout=1.0, max_samples=win)
        if not chunk:
            continue

        data = np.asarray(chunk, dtype=np.float32)
        if data.ndim == 1:
            data = data.reshape(-1, n_ch)

        if data.shape[1] != n_ch:
            raise RuntimeError(f"Chunk inattendu: {data.shape}, n_ch attendu={n_ch}")

        #gros chunk=on garde que derniers échantillons
        if data.shape[0] >= win: #(n_samples_chunk, n_ch) #1250 échantillons=5s à 250 Hz?
            data = data[-win:] #5dernieres secondes

        m = data.shape[0] #nb echantillons a ecrire

        #ecriture dans buffer
        end = widx + m
        if end <= win: #pas depassement
            buf[widx:end] = data
        else: #au bout du buffer reprise au debut
            first = win - widx #places restantes fin buffer
            buf[widx:] = data[:first] #fin du buffer devient premier du chunk
            buf[:end - win] = data[first:] #reste des echantillions va au debut

        widx = end % win
        filled = min(win, filled + m) #echantillons ok deja dans buffer
        since_emit += m

        # Tant que pas 5 secondes, on n'émet rien
        if filled < win:
            continue

        #emission toutes les 500 ms
        while since_emit >= hop:
            #oldest -> newest
            window = np.concatenate((buf[widx:], buf[:widx]), axis=0).copy()
            since_emit -= hop
            yield window  #shape: (win, n_ch)


# if __name__ == "__main__":
#     inlet, fs, n_ch = connect_lsl("EEGStream")
#
#     for W in iter_sliding_windows(inlet, fs, n_ch, win_s=5.0, hop_s=0.5):
#         outlet.push_chunk(W.tolist()) #push du chunk toutes les 500ms dans stream
#         print("Fenêtre prête:", W.shape)