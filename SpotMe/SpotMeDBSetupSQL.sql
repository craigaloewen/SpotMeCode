CREATE TABLE [dbo].[Exercises]
(
	[exerciseId] INT IDENTITY (1, 1) NOT NULL,
  [exerciseName] NVARCHAR(MAX) NULL, 
  [contractedFormData] NVARCHAR(MAX) NULL, 
  [extendedFormata] NVARCHAR(MAX) NULL
	CONSTRAINT [PK_dbo.Exercises] PRIMARY KEY CLUSTERED ([exerciseId] ASC)
)

CREATE TABLE [dbo].[Classifiers]
(
	[classifierId] INT IDENTITY (1, 1) NOT NULL,
  [classifierName] NVARCHAR(MAX) NULL, 
	[exerciseId] INT NOT NULL, 
  CONSTRAINT [PK_dbo.Classifiers] PRIMARY KEY CLUSTERED ([classifierId] ASC),
	CONSTRAINT [FK_dbo.Classifiers_dbo.Exercises_exerciseId] FOREIGN KEY ([exerciseId]) REFERENCES [dbo].[Exercises] ([exerciseId]) ON DELETE CASCADE 
)

INSERT INTO [dbo].[Exercises] ([exerciseName],[contractedFormData],[extendedFormata]) 
VALUES ('crouch', '1.28;1.32;4.567;23.98', '1.28;1.32;4.567;23.98')

INSERT INTO [dbo].[Classifiers] ([classifierName],[exerciseId]) 
VALUES ('crouch', '1')