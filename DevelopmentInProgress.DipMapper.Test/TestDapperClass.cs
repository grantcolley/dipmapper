using System;
using System.Collections.Generic;

namespace DevelopmentInProgress.DipMapper.Test
{
    public class TestDapperClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public IEnumerable<TestDapperClass> TestDataClasses { get; set; }
    }
}
