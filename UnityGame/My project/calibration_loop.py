############################################
# SCRIPT UNITY -> PYTHON.
# C'EST LE SCRIPT A MODIFIER QUI RECEVRA
# LES DEMANDES DE CALIBRATION DE UNITY.
# POTENTIELLEMENT COOL POUR CALIBRER EN DEBUT DE JEU.
###########################################

# to get python prints in Unity console
import sys
sys.stdout.reconfigure(line_buffering=True)
import time
from pylsl import resolve_byprop, StreamInlet
from collect_calibration import collect_main, connect_lsl

def main():
    # leaving some time for Unity CalibrationScene to start the LSL data stream
    time.sleep(1)
    
    stream_names = ['Left hand movement', 'Right hand movement']
    streams = {}
    inlets = {}
    for stream_name in stream_names:
        streams[stream_name] = resolve_byprop("name", stream_name, timeout=5)
        if not streams[stream_name]:
            raise RuntimeError(f"Stream {stream_name} not found")
        inlets[stream_name] = StreamInlet(streams[stream_name][0])

    inletCollection, fs, n_ch =connect_lsl()
    
    running = False
    print("Waiting for control signals...")
    while True:
        for stream_name,inlet in inlets.items():
            sample, ts = inlet.pull_sample(timeout=0.01)
            if sample is None:
                continue
            value = bool(sample[0])
            isLeft = (stream_name == 'Left hand movement')
    
            if value and not running:
                running = True
                print(f"START RECORDING {stream_name}")
                if isLeft:
                    main = 0
                else:
                    main = 1
                collect_main(inletCollection, fs, n_ch, main=main, npz_path=f"dataset_calibration.npz")
            elif not value and running:
                running = False
                print(f"STOP RECORDING {stream_name}")
            
if __name__ == "__main__":
    main()
