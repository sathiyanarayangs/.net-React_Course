using PatternsLab.Day1;
using PatternsLab.Day2;
using PatternsLab.Day2.Vehicles;
using PatternsLab.Day2.Observer;
using PatternsLab.Day3.Strategy;
using PatternsLab.Day3.Repository;
using PatternsLab.Day3.AdapterFacade;
using PatternsLab.Day4;
using PatternsLab.Day4.Attributes;
using PatternsLab.Day5;

Console.WriteLine("===================================================");
Console.WriteLine(" Week 1 Patterns Lab — Exceptions, Async, Patterns,");
Console.WriteLine(" TPL/Reflection/Attributes, SOLID basics, Sorting");
Console.WriteLine("===================================================\n");

// ---- Day 1 ----
ExceptionsDemo.RunWithdrawalScenario();
Console.WriteLine();
ExceptionsDemo.ParseAndCompute("not-a-number");
ExceptionsDemo.ParseAndCompute("2");
Console.WriteLine();

TempFileManagerDemo.Run();
Console.WriteLine();

await AsyncDemo.RunAsync();
Console.WriteLine();

// ---- Day 2 ----
LoggerDemo.Run();
Console.WriteLine();

VehicleFactoryDemo.Run();
Console.WriteLine();

StockTickerDemo.Run();
Console.WriteLine();

// ---- Day 3 ----
PaymentStrategyDemo.Run();
Console.WriteLine();

RepositoryDemo.Run();
Console.WriteLine();

AdapterFacadeDemo.Run();
Console.WriteLine();

// ---- Day 4 ----
TplDemo.Run();
Console.WriteLine();

ReflectionDemo.Run();
Console.WriteLine();

AttributeValidatorDemo.Run();
Console.WriteLine();

// ---- Day 5 ----
InterfaceVsAbstractDemo.Run();
Console.WriteLine();

StaticVsInstanceDemo.Run();
Console.WriteLine();

SortingDemo.Run();
Console.WriteLine();

Console.WriteLine("===================================================");
Console.WriteLine(" Done. See PATTERNS.md for the pattern -> future-week map.");
Console.WriteLine("===================================================");
