import numpy as np

path = "dataset_calibration.npz"
data = np.load(path, allow_pickle=True)

print("Clés:", data.files)
for k in data.files:
    arr = data[k]
    if isinstance(arr, np.ndarray):
        print(f"{k}: shape={arr.shape}, dtype={arr.dtype}")
    else:
        print(f"{k}: type={type(arr)}")

# Récupération standard
X = data["X"]          # (n_trials, n_samples, n_ch)
y = data["y"] if "y" in data.files else None
fs = float(data["fs"]) if "fs" in data.files else None

print("\nRésumé:")
print("X:", X.shape, X.dtype)
if y is not None:
    print("y:", y.shape, y.dtype, "classes:", np.unique(y, return_counts=True))
print("fs:", fs)



import matplotlib.pyplot as plt
import numpy as np

def plot_trial(Xtrial, fs=None, title="Trial"):
    n, ch = Xtrial.shape
    t = np.arange(n) / fs if fs else np.arange(n)

    plt.figure()
    offset = 0.0
    for k in range(ch):
        plt.plot(t, Xtrial[:, k] + offset)
        offset += 2.5 * np.std(Xtrial[:, k])  # décalage vertical automatique
    plt.title(title)
    plt.xlabel("Time (s)" if fs else "Samples")
    plt.ylabel("Amplitude (offset)")
    plt.tight_layout()
    plt.show()

trial = 0
Xtrial = X[trial]
label = int(y[trial]) if y is not None else None

plot_trial(Xtrial, fs, title=f"Trial {trial} | label={label}")