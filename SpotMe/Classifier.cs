namespace SpotMe
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Classifier
    {
        public int classifierId { get; set; }

        public string classifierName { get; set; }

        public int exerciseId { get; set; }

        public virtual Exercise Exercis { get; set; }
    }
}
