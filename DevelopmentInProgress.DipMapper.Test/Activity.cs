using System;
using System.Collections.Generic;

namespace DevelopmentInProgress.DipMapper.Test
{
    public class Activity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Number { get; set; }
        public DateTime Date { get; set; }
        public DateTime? NullableDate { get; set; }
        public IEnumerable<Activity> Activities_1 { get; set; }
        public IList<Activity> Activities_2 { get; set; }
    }
}
