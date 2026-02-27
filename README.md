# STRUCTURE DU REPO

[1] ASSETS/PACKAGES/PROJECTSETTINGS -> Specifics de Unity.
[2] MIREPNET -> tout ce qui contient le modele et des scripts de base de MiRepNet dont FINETUNE.PY -> Principale entree pour finetune le modele sur des données supplementaires. Pour le moment ca sauvegarde pas le modele finetuné, A CHANGER!!!
Sinon on prend un autre modele.
[3] Calibration.py -> Appelé de Unity vers python, utile si on veut calibrer depuis Unity. Bon exemple de talk LSL entre Unity -> python.
[4] Input_sender.py -> appele du Unity depuis python Python -> Unity, elle sera ici la boucle d'inférence de notre modèle.
#test