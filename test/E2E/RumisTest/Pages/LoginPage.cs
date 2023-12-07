using OpenQA.Selenium;
using RumisTest.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using System.Configuration;

namespace RumisTest.Pages
{
    class LoginPage : BaseRunner
    {
        public static void login(String AdditionalURL, String errorMessage)
        {
            string targetLink = ConfigurationManager.AppSettings["BaseMapURL"];
            try {
                driver.Navigate().GoToUrl(targetLink + AdditionalURL);
            } catch (Exception e) {
                Console.WriteLine("ERROR: Radās kļūda atverot RUMIS vidi!");
                Console.WriteLine(e);
                Assert.Fail("Neizdevās atvērt" + errorMessage);
            }
        }
    }
}
