// See https://aka.ms/new-console-template for more information
using OrangeTask;

Console.WriteLine("This application is a PoC on generating and reading log files for OBS presentation. If you are not authorized to use this application, please close it now.");

while (true)
{
    Console.WriteLine("Do you need to generate logs or read log file? <Generate, Read, Exit>");

    string Command = Console.ReadLine();
    try
    {
        switch (Command.ToLower().Trim())
        {
            case "exit":
                return;
                break;
            case "generate":
                LogGenerator.RunGenerateWizard();
                break;
            case "read":
                LogGenerator.RunReadWizard();
                break;
            default:
                CommandLine.Write("Incorrect command", Command, CommandLine.Status.Fail);
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }   
   
}



