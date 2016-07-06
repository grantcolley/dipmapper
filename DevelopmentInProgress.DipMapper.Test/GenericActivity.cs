using System.Collections.Generic;

namespace DevelopmentInProgress.DipMapper.Test
{
    public class GenericActivity<T>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public T GenericProperty { get; set; }
        public IEnumerable<GenericActivity<T>> Activities_1 { get; set; }
        public IList<GenericActivity<T>> Activities_2 { get; set; }
        public T[] TypeArray { get; set; }
    }
}
