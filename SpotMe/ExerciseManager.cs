using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotMe
{
    static class ExerciseManager
    {
        public static void addOrUpdateExercise(Exercise ex)
        {
            ex.exerciseName = ex.exerciseName.ToUpper();

            using (var db = new SpotMeDBContext())
            {
                try
                {
                    // Add Exercise
                    db.Exercises.Add(ex);
                    db.SaveChanges();
                }
                catch (Exception e)
                {

                }
            }
        }

        public static void deleteExercise(Exercise ex)
        {
            ex.exerciseName = ex.exerciseName.ToUpper();

            using (var db = new SpotMeDBContext())
            {
                try
                {
                    db.Exercises.Remove(ex);
                    db.SaveChanges();
                }
                catch (Exception e)
                {

                }
            }
        }
    }
}
