import os
import scipy.io
import numpy as np

data_dir = "./data/BNCI2014004/"
trial_length_sec = 4

all_trials = []
all_labels = []

for subject in range(1, 10):  # Subjects 1–9
    file_name = f"B0{subject}E.mat"
    file_path = os.path.join(data_dir, file_name)

    print("Loading:", file_path)
    mat = scipy.io.loadmat(file_path)
    data_struct = mat["data"][0, 0]

    X = data_struct["X"][0, 0][:,:3]          # samples × channels
    trial = data_struct["trial"][0, 0]  # trial start indices
    y = data_struct["y"][0, 0]
    fs = int(data_struct["fs"][0, 0])

    trial_length_samples = trial_length_sec * fs

    print(f"Subject {subject} trials found:", len(trial))

    for i in range(len(trial)):
        start = trial[i][0]
        end = start + trial_length_samples

        segment = X[start:end, :].T  # (channels, time)
        label = y[i][0]

        all_trials.append(segment)
        all_labels.append(label)

X_final = np.array(all_trials)
y_final = np.array(all_labels)

print("Final dataset shape:", X_final.shape)
print("Final labels shape:", y_final.shape)

np.save(os.path.join(data_dir, "X.npy"), X_final)
np.save(os.path.join(data_dir, "labels.npy"), y_final)

print("Saved X.npy and labels.npy")