###########################################
# SCRIPT PYTHON -> UNITY.
# C'EST LE SCRIPT A MODIFIER QUI ENVERRA 
# LES INFERENCES DE MIREPNET A UNITY.
###########################################

# to get python prints in Unity console
import sys
sys.stdout.flush()
from pylsl import StreamInfo, StreamOutlet
import time

def main():
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

    print("LSL outlet started")
    input_ = True
    while True:
       time.sleep(2)
       input_ = not input_
       outlet.push_sample([int(input_)])
       print(f'Sent {input_} to Unity', flush=True)

if __name__ == "__main__":
    main()
