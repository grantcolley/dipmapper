using System;
using System.Collections.Generic;

namespace DevelopmentInProgress.DipMapper.Test
{
    public class GenericActivityOra<T>
    {
        public GenericActivityOra()
        {
            Activities_1 = new List<GenericActivityOra<T>>();
            Activities_2 = new List<GenericActivityOra<T>>();
            GroupIds = new T[3];
            Description = "Desc...";
            AssociatedActivityId = 3;
            ParentActivityId = 5;
        }

        // Supported by DipMaper
        public int Id { get; set; }
        public string Name { get; set; }
        public double Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public ActivityTypeEnum ActivityType { get; set; }

        // Supported by DipMapper if T is a value type.
        public T GenericProperty { get; set; }

        // Not supported by DipMapper
        public GenericActivityOra<T> ParentActivity { get; set; }
        public IEnumerable<GenericActivityOra<T>> Activities_1 { get; set; }
        public IList<GenericActivityOra<T>> Activities_2 { get; set; }
        public T[] GroupIds { get; set; }
        internal string Description { get; set; }
        protected int AssociatedActivityId { get; set; }
        private int ParentActivityId { get; set; }
    }
}
