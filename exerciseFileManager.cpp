#include "exerciseFileManager.h"

CExerciseFileManager::CExerciseFileManager() {
	numOfExercises = 0;
}

CExerciseFileManager::~CExerciseFileManager() {

}



std::string CExerciseFileManager::spaceToDash(std::string text) {
	for (std::string::iterator it = text.begin(); it != text.end(); ++it) {
		if (*it == ' ') {
			*it = '-';
		}
	}
	return text;
}
std::string CExerciseFileManager::dashToSpace(std::string text) {
	for (std::string::iterator it = text.begin(); it != text.end(); ++it) {
		if (*it == '-') {
			*it = ' ';
		}
	}
	return text;
}

bool CExerciseFileManager::saveExercise() {
	if (currentExercise.name == "") {
		return false;
	}

	if (currentExercise.numOfClassifications != currentExercise.classificationLabels.size()) {
		return false;
	}
	std::ofstream outputFile;
	outputFile.open(spaceToDash(currentExercise.name) + ".txt");
	
	if (!outputFile.is_open()) {
		return false;
	}

	outputFile << currentExercise.numOfClassifications << std::endl;
	for (int i = 0; i < currentExercise.numOfClassifications; i++) {
		outputFile << currentExercise.classificationLabels[i] << std::endl;
		outputFile << currentExercise.classificationMessages[i] << std::endl;
		outputFile << currentExercise.jointNumberList[i] << std::endl;
		outputFile << currentExercise.arrowDirectionList[i] << std::endl;
	}
	outputFile.close();

	if (!trainingData->save(spaceToDash(currentExercise.name) + ".csv")) {
		return false;
	}

	return true;
}

bool CExerciseFileManager::loadExercise(std::string exerciseName) {

	std::ifstream inputFile;
	std::string fileName;
	

	fileName = spaceToDash(exerciseName);


	inputFile.open(fileName + ".txt");

	if (!inputFile.is_open()) {
		return false;
	}

	if (!trainingData->load(spaceToDash(fileName) + ".csv")) {
		trainingData->clear();
		trainingData->save(spaceToDash(fileName) + ".csv");
		trainingData->load(spaceToDash(fileName) + ".csv");
	}

	std::string input;

	exerciseData outExercise;

	outExercise.name = exerciseName;
	getline(inputFile, input);
	outExercise.numOfClassifications = atoi(input.c_str());
	for (int i = 0; i < outExercise.numOfClassifications; i++) {
		getline(inputFile, input);
		outExercise.classificationLabels.push_back(input);
		getline(inputFile, input);
		trainingData->addClass(i + 1, input);
		outExercise.classificationMessages.push_back(input);
		getline(inputFile, input);
		outExercise.jointNumberList.push_back(atoi(input.c_str()));
		getline(inputFile, input);
		outExercise.arrowDirectionList.push_back(atoi(input.c_str()));
		
	}

	inputFile.close();

	currentExercise =  outExercise;

	trainAndTestPipeline();

	return true;

}

bool CExerciseFileManager::trainAndTestPipeline() {
	bool result;

	if (trainingData->getNumSamples() < currentExercise.numOfClassifications) {
		return false;
	}

	GRT::ClassificationData testData = trainingData->partition(90);

	result = pipelinePointer->train((*trainingData));
	if (!result) {
		return false;
	}

	pipelinePointer->test(testData);


	lastPipelineStatistics = pipelinePointer->getTestFMeasure();

	trainingData->merge(testData);

	return true;
}

bool CExerciseFileManager::addClassification(std::string classificationLabel, std::string classificationMessage, int inJointNumber, int inArrowDirection) {
	if (currentExercise.name == "") {
		return false;
	}
	currentExercise.numOfClassifications++;
	currentExercise.classificationLabels.push_back(classificationLabel);
	currentExercise.classificationMessages.push_back(classificationMessage);
	currentExercise.jointNumberList.push_back(inJointNumber);
	currentExercise.arrowDirectionList.push_back(inArrowDirection);

	trainingData->addClass(trainingData->getMaximumClassLabel()+1, classificationLabel);
	trainAndTestPipeline();
	return saveExercise();
}

bool CExerciseFileManager::removeClassification(int classificationNumber) {
	if (currentExercise.name == "") {
		return false;
	}
	if (classificationNumber >= currentExercise.numOfClassifications || currentExercise.numOfClassifications == 0) {
		return false;
	}
	currentExercise.numOfClassifications--;
	std::vector<std::string>::iterator labelIterator, messageIterator;
	std::vector<int>::iterator jointIterator, arrowIterator;
	labelIterator = currentExercise.classificationLabels.begin();
	messageIterator = currentExercise.classificationMessages.begin();
	jointIterator = currentExercise.jointNumberList.begin();
	arrowIterator = currentExercise.arrowDirectionList.begin();
	for (int i = 0; i < classificationNumber; i++) {
		labelIterator++;
		messageIterator++;
		jointIterator++;
		arrowIterator++;
	}
	currentExercise.classificationLabels.erase(labelIterator);
	currentExercise.classificationMessages.erase(messageIterator);
	currentExercise.jointNumberList.erase(jointIterator);
	currentExercise.arrowDirectionList.erase(arrowIterator);

	trainingData->removeClass(classificationNumber+1);
	for (int i = classificationNumber+1; i < trainingData->getMaximumClassLabel()+1; i++) {
		trainingData->relabelAllSamplesWithClassLabel(i, i-1);
	}

	trainAndTestPipeline();
	return saveExercise();
}

bool CExerciseFileManager::populateExercises() {
	std::ifstream inputFile;
	inputFile.open("ExerciseList.txt");

	exerciseNameList.clear();
	numOfExercises = 0;

	if (inputFile.is_open()) {
		std::string inputString;

		while (getline(inputFile, inputString)) {
			exerciseNameList.push_back(dashToSpace(inputString));
			numOfExercises++;
		}
	}
	inputFile.close();

	if (numOfExercises > 0) {
		loadExercise(exerciseNameList[0]);
	}
	else {
		return false;
	}

	return true;


}

bool CExerciseFileManager::addNewExercise(std::string inExerciseName) {
	std::ifstream inputFile;
	inputFile.open("ExerciseList.txt");

	if (!inputFile.is_open()) {
		return false;
	}


	std::string lineData;
	std::string inExerciseDashName = spaceToDash(inExerciseName);
	while (getline(inputFile, lineData)) {
		if (lineData == inExerciseDashName) {
			return false;
		}
	}

	std::ofstream outputFile;
	outputFile.open("ExerciseList.txt",std::ios::app);
	if (!outputFile.is_open()) {
		return false;
	}
	outputFile << spaceToDash(inExerciseName) << std::endl;
	outputFile.close();
	exerciseNameList.push_back(inExerciseName);
	numOfExercises++;

	exerciseData newExercise;
	newExercise.name = inExerciseName;
	newExercise.numOfClassifications = 0;
	newExercise.classificationLabels.clear();
	newExercise.classificationMessages.clear();
	newExercise.arrowDirectionList.clear();
	newExercise.jointNumberList.clear();
	trainingData->clear();

	currentExercise = newExercise;
	saveExercise();
	return true;
}

bool CExerciseFileManager::deleteExercise(exerciseData inExercise) {
	std::ifstream inputFile;
	inputFile.open("ExerciseList.txt");
	std::ofstream outputFile;
	outputFile.open("ExerciseListTemp.txt");

	if (!inputFile.is_open() || !outputFile.is_open()) {
		return false;
	}


	std::string lineData;
	std::string inExerciseDashName = spaceToDash(inExercise.name);
	bool found = false;
	while (getline(inputFile, lineData)) {
		if (lineData != inExerciseDashName) {
			outputFile << lineData << std::endl;
		}
		else {
			found = true;
		}
	}


	if (!found) {
		inputFile.close();
		outputFile.close();
		std::remove("ExerciseListTemp.txt");
		return false;
	}

	inputFile.close();
	outputFile.close();

	std::string exerciseClassifierName = inExerciseDashName + ".txt";
	std::string trainingDataFile = inExerciseDashName + ".csv";

	std::remove(exerciseClassifierName.c_str());
	std::remove(trainingDataFile.c_str());
	std::remove("ExerciseList.txt");
	std::rename("ExerciseListTemp.txt", "ExerciseList.txt");

	

	if (numOfExercises > 0) {
		loadExercise(exerciseNameList[0]);
	}
	else {
		currentExercise.name = "";
	}

	populateExercises();

	return true;

}

bool CExerciseFileManager::deleteCurrentExercise() {
	

	return deleteExercise(currentExercise);

}