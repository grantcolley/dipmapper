using System;
using System.Collections.Generic;

namespace DevelopmentInProgress.DipMapper.Test
{
    public class GenericActivity<T>
    {
        public GenericActivity()
        {
            Activities_1 = new List<GenericActivity<T>>();
            Activities_2 = new List<GenericActivity<T>>();
            GroupIds = new T[3];
            Description = "Desc...";
            AssociatedActivityId = 3;
            ParentActivityId = 5;
        }

        // Supported by DipMap ORM
        public int Id { get; set; }
        public string Name { get; set; }
        public double Level { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public ActivityTypeEnum ActivityType { get; set; }

        // Supported by DipMap ORM if T is a value type.
        public T GenericProperty { get; set; }

        // Not supported by DipMap ORM
        public GenericActivity<T> ParentActivity { get; set; }
        public IEnumerable<GenericActivity<T>> Activities_1 { get; set; }
        public IList<GenericActivity<T>> Activities_2 { get; set; }
        public T[] GroupIds { get; set; }
        internal string Description { get; set; }
        protected int AssociatedActivityId { get; set; }
        private int ParentActivityId { get; set; }
    }
}
