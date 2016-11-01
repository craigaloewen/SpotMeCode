#ifndef CEXERCISEFILEMANAGER_H
#define CEXERCISEFILEMANAGER_H
#include <fstream>
#include <vector>
#include "stdlib.h"
#include <string>
#include "GRT\GRT.h"


class CExerciseFileManager {

public:

	struct {
		int numOfClassifications;
		std::string name;
		std::vector<std::string> classificationLabels;
		std::vector<std::string> classificationMessages;
		std::vector<int> jointNumberList;
		std::vector<int> arrowDirectionList;
	} typedef exerciseData;

	exerciseData currentExercise;

	std::vector<std::string> exerciseNameList;
	int numOfExercises;

	CExerciseFileManager();
	~CExerciseFileManager();

	bool saveExercise();
	bool loadExercise(std::string exerciseName);
	bool addClassification(std::string classificationLabel, std::string classificationMessage, int inJointNumber, int inArrowDirection);
	bool removeClassification(int classificationNumber);
	bool populateExercises();
	bool addNewExercise(std::string inExerciseName);
	bool deleteExercise(exerciseData inExercise);
	bool deleteCurrentExercise();

	bool trainAndTestPipeline();

	
	GRT::ClassificationData* trainingData;
	GRT::GestureRecognitionPipeline* pipelinePointer;
	GRT::VectorFloat lastPipelineStatistics;


private:

	std::string spaceToDash(std::string text);
	std::string dashToSpace(std::string text);
	
	





};

#endif