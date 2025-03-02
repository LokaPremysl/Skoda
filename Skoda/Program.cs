using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Forms;

namespace Cars
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            View view = new View();
            Presenter presenter = new Presenter(view);
            Application.Run(view);
        }
    }
}