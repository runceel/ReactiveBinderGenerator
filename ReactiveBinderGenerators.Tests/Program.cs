using System;

namespace ReactiveBinderGenerators.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }

    public class Person
    {
        public string Name { get; set; }
    }

    [BindFrom(typeof(Person))]
    public class PersonViewModel
    {

    }
}
