namespace SpotMe
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Exercises")]
    public partial class Exercise
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Exercise()
        {
            Classifiers = new HashSet<Classifier>();
        }

        [Key]
        public int exerciseId { get; set; }

        public string exerciseName { get; set; }

        public string contractedFormData { get; set; }

        [NotMapped]
        public double[] contractedForm
        {
            get
            {
                return Array.ConvertAll(contractedFormData.Split(';'), Double.Parse);
            }
            set
            {
                double[] _data = value;
                contractedFormData = String.Join(";", _data.Select(p => p.ToString()).ToArray());
            }
        }

        public string extendedFormata { get; set; }

        [NotMapped]
        public double[] extendedForm
        {
            get
            {
                return Array.ConvertAll(extendedFormata.Split(';'), Double.Parse);
            }
            set
            {
                double[] _data = value;
                extendedFormata = String.Join(";", _data.Select(p => p.ToString()).ToArray());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Classifier> Classifiers { get; set; }
    }
}
