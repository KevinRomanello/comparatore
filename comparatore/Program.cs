using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectComparisonTest
{
    // Classe di test con vari tipi di membri
    public class TestClass
    {
        // Fields pubblici
        public string PublicField = "Test";

        // Fields privati
        private int _privateField = 42;

        // Properties pubbliche
        public string PublicProperty { get; set; } = "Hello";
        public List<int> Numbers { get; set; } = new List<int> { 1, 2, 3 };

        // Properties private
        private DateTime _privateProperty { get; set; } = DateTime.Now;

        // Costruttore che permette di modificare i valori
        public TestClass(string publicField = null, int privateField = 42,
                        string publicProperty = null, List<int> numbers = null)
        {
            if (publicField != null) PublicField = publicField;
            _privateField = privateField;
            if (publicProperty != null) PublicProperty = publicProperty;
            if (numbers != null) Numbers = numbers;
        }
    }

    public class ObjectComparator
    {
        public bool Compare(object obj1, object obj2)
        {
            try
            {
                // Verifica che entrambi gli oggetti non siano null e siano dello stesso tipo
                if (obj1 == null || obj2 == null) return false;
                if (obj1.GetType() != obj2.GetType()) return false;

                Type type = obj1.GetType();

                // CONFRONTO FIELDS
                // Ottiene tutti i fields (pubblici e privati) dell'oggetto
                var fields = type.GetFields(BindingFlags.Public |
                                          BindingFlags.NonPublic |
                                          BindingFlags.Instance);

                // Confronta ogni field
                foreach (var field in fields)
                {
                    // Legge i valori dei fields tramite reflection
                    var value1 = field.GetValue(obj1);
                    var value2 = field.GetValue(obj2);

                    // Se i valori sono diversi, logga la differenza e ritorna false
                    if (!AreValuesEqual(value1, value2))
                    {
                        Console.WriteLine($"Differenza trovata in {field.Name}");
                        return false;
                    }
                }

                // CONFRONTO PROPERTIES
                // Ottiene tutte le properties (pubbliche e private) dell'oggetto
                var properties = type.GetProperties(BindingFlags.Public |
                                                  BindingFlags.NonPublic |
                                                  BindingFlags.Instance);

                // Confronta ogni property
                foreach (var prop in properties)
                {
                    // Salta le properties di sola scrittura
                    if (!prop.CanRead) continue;

                    // Legge i valori delle properties tramite reflection
                    var value1 = prop.GetValue(obj1);
                    var value2 = prop.GetValue(obj2);

                    // Se i valori sono diversi, logga la differenza e ritorna false
                    if (!AreValuesEqual(value1, value2))
                    {
                        Console.WriteLine($"Differenza trovata in {prop.Name}");
                        return false;
                    }
                }

                // Se arriviamo qui, tutti i confronti sono andati bene
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Errore durante il confronto: {ex.Message}");
            }
        }

        /// Confronta due valori gestendo casi speciali (liste, date, ecc.)
        private bool AreValuesEqual(object value1, object value2)
        {
            // Gestione null: due null sono uguali, un null e un non-null sono diversi
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // GESTIONE LISTE
            if (value1 is IList list1 && value2 is IList list2)
            {
                // Verifica che le liste abbiano la stessa lunghezza
                if (list1.Count != list2.Count) return false;

                // Confronta ogni elemento delle liste ricorsivamente
                for (int i = 0; i < list1.Count; i++)
                {
                    var element1 = list1[i];
                    var element2 = list2[i];
                    if (!AreValuesEqual(element1, element2))
                        return false;
                }
                return true;
            }

            // GESTIONE TIPI PRIMITIVI (int, string, bool, ecc.)
            if (value1.GetType().IsPrimitive || value1 is string)
                return value1.Equals(value2);

            // GESTIONE DATE
            // Confronta le date ignorando i secondi per evitare falsi negativi
            if (value1 is DateTime date1 && value2 is DateTime date2)
            {
                return date1.Date == date2.Date &&
                       date1.Hour == date2.Hour &&
                       date1.Minute == date2.Minute;
            }

            // Per tutti gli altri tipi di oggetti usa Equals standard
            return value1.Equals(value2);
        }       
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Test Comparatore Oggetti\n");

            // Test 1: Oggetti identici
            Console.WriteLine("Test 1: Oggetti identici");
            var obj1 = new TestClass();
            var obj2 = new TestClass();
            TestComparison(obj1, obj2);

            // Test 2: Oggetti diversi
            Console.WriteLine("\nTest 2: Oggetti con valori diversi");
            var obj3 = new TestClass(
                publicField: "Modificato",
                privateField: 100,
                publicProperty: "World",
                numbers: new List<int> { 4, 5, 6 }
            );
            TestComparison(obj1, obj3);

            // Test 3: Confronto con null
            Console.WriteLine("\nTest 3: Confronto con null");
            TestComparison(obj1, null);

            Console.WriteLine("\nPremi un tasto per uscire...");
            Console.ReadKey();
        }

        static void TestComparison(object obj1, object obj2)
        {
            var comparator = new ObjectComparator();
            bool result = comparator.Compare(obj1, obj2);
            Console.WriteLine($"Risultato confronto: {(result ? "Oggetti uguali" : "Oggetti diversi")}");
            Console.WriteLine(new string('-', 50));
        }
    }
}
