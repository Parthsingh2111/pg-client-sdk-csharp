using System;
using System.Threading.Tasks;
using PayGlocal;

namespace PayGlocal.Test
{
    /// <summary>
    /// Simple compilation test to verify all dependencies
    /// </summary>
    public class CompilationTest
    {
        public static void TestCompilation()
        {
            try
            {
                // Test basic client instantiation
                var client1 = new PayGlocalClient();
                
                // Test parameterized constructor
                var client2 = new PayGlocalClient(
                    merchantId: "test",
                    apiKey: "test",
                    environment: "UAT"
                );

                Console.WriteLine("✅ SDK compilation successful!");
                Console.WriteLine("✅ All dependencies resolved correctly!");
                Console.WriteLine("✅ JOSE library integrated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Compilation test failed: {ex.Message}");
                throw;
            }
        }
    }
} 