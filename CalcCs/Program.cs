class Program
{
	static void Main()
	{
		string? line;
		while ((line = Console.ReadLine()) != null)
		{
			try
			{
				var result = CalcCs.Calc(line);
				Console.WriteLine("-> {0}", result);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.GetType());
			}
		}
	}
}