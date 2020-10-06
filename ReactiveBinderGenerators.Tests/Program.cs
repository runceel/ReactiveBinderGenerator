using System;
using ReactiveBinderGenerators;

namespace ReactiveBinderGenerators.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new Person { Name = "Kazuki" };
            var vm = new PersonViewModel(p);
            Console.WriteLine(vm.Name.Value);
            vm.Name.Value = "Ota";
            Console.WriteLine(p.Name);
        }
    }

    public class Person
    {
        public string Name { get; set; }
    }

    [BindFrom(typeof(Person))]
    public partial class PersonViewModel
    {

    }
}
