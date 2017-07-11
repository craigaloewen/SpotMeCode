SpotMeML.cs
•	Initialize machine learning based on exercise(Exercise inExercise)
returns bool for success
•	Detect when a user has paused(Body inBody)
returns bool for paused or not
•	Get form critique for an inputted skeleton(Body inBody)
returns the classifier number

Exercise.cs
•	Get/Set exercise name [ using a string]
•	Get/Set exercise good form (contracted and extended) [using double[] ]
•	Get/Set list of classifiers [ using a List<Classifier> ]

Classifier.cs
•	Get/set classifier name [ using a string ]

ExerciseFileIO.cs (Could be static?)
•	Save Exercise(string exerciseName, exercise inExercise)
returns bool for success
•	Load Exercise(string exerciseName, out exercise outExercise)
returns bool for success
•	Delete Exercise(string exerciseName)
returns bool for success
•	Modify Exercise [Could be done by just giving access to exercise and then saving it]
•	Add classifier (Exercise inExercise, Classifier inClassifier)
returns bool for success
•	Modify classifier(Exercise inExercise, Classifier inClassifier)
•	Delete classifier(Exercise inExercise, Classifier inClassifier)
returns bool for success
•	Add classifier to current exercise (Classifier inClassifier)
returns bool for success
•	Modify classifier to current exercise (Classifier inClassifier) 
•	Delete classifier to current exercise (Classifier inClassifier)
returns bool for success
•	Enumerate Exercise List(out List<string> exerciseNames, out double[][] skeletonData) [Include names, and good form skeleton for thumbnail]
returns bool for success
•	Get Current Exercise() 
returns Exercise

MainWindow.cs (The form for displaying the window)
•	Display skeleton in small thumbnail fomats(bodyDouble inBodyDouble)
returns void

Feedback.cs
•	Save skeleton of that rep(Body inBody)
returns bool
•	Get/Set list of rep skeletons done in that set [ using a List<Body> ]
•	POSSIBLE EXTENSION: Only get list of skeletons done with bad form, etc.
