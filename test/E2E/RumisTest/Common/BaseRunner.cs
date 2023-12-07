using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using SeleniumExtras.WaitHelpers;
using MySqlConnector;

namespace RumisTest.Common
{
    public class BaseRunner
    {
        public static IWebDriver driver { get; set; }

        // Gaida, līdz atrod elementu (līdz tas parādās uz lapas)
        public static Boolean WaitUntilElementIsVisibleCss(string CssSelectorString)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(45));
                wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(CssSelectorString)));
                return true;
            } catch(Exception e)
            {
                Console.WriteLine("ERROR: Elements ar CSS selector string: '" + CssSelectorString + "' nav pieejams.");
                Console.WriteLine(e);
                return false;
            }
        }

        public void W(string message)
        {
            Console.WriteLine(message);
        }

        public void W(Exception exc)
        {
            Console.WriteLine(exc);
        }

        public Process ExecuteAsAdmin(String fileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
            return proc;
        }

        public static void initializeChrome()
        {
            ChromeOptions chromeOptions = new ChromeOptions();

            //chromeOptions.AcceptInsecureCertificates = true;
            chromeOptions.AddArguments("--ignore-certificate-errors");

            driver = new ChromeDriver(chromeOptions);
            driver.Manage().Window.Size = new Size(1920,1080);
            driver.Manage().Window.Maximize();
        }

        // Gaida, līdz atrod elementu (līdz tas parādās uz lapas)
        public static Boolean WaitUntilElementIsVisibleCss(string CssSelectorString, TimeSpan timeout)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, timeout);
                wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(CssSelectorString)));
                return true;
            } catch(Exception e)
            {
                Console.WriteLine("ERROR: Elements ar CSS selector string: '" + CssSelectorString + "' nav pieejams ");
                Console.WriteLine(e);
                return false;
            }
        }

        public static void WaitWhileCurtainIsOn(int timeoutSeconds)
        {
            int iterationTimeout = 0;
            IWebElement curtainElement = null;
            Boolean curtainIsOn = true;
            while(curtainIsOn)
            {
                try
                {
                    curtainElement = driver.FindElement(By.XPath("//*[@class=\"app-curtain ng-star-inserted\"]"));
                }
                catch(Exception e)
                {
                    curtainIsOn = false;
                }
                if (curtainElement == null && curtainIsOn == false)
                {
                    System.Console.WriteLine("INFO: \"Aizkaru\" elements bija aktīvs " + (iterationTimeout/1000) + " sekundes.\n" );
                    break;
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                    iterationTimeout += 1000;
                    int delta = (timeoutSeconds * 1000) - iterationTimeout;
                    if (delta < 0 )
                    {
                        Assert.Fail("\"Aizkaru\" elements bija aktīvs parāk ilgi (pārsniegts timeout laiks"+ timeoutSeconds + "). Lapa ir neaktīva.\n");
                    }
                }
                
            }
        }

        public static Boolean WaitUntilElementIsVisibleXPath(string XPathString)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(45));
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(XPathString)));
                return true;
            } catch (Exception e)
            {
                Console.WriteLine("ERROR: Elements ar Xpath: '" + XPathString + "' nav pieejams ");
                Console.WriteLine(e);
                return false;
            }
        }

        public static Boolean WaitUntilElementIsClickable(IWebElement element)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(45));
            try
            {
                wait.Until(ExpectedConditions.ElementToBeClickable(element));
                return true;
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("ERROR: Elements '" + element + "' nav pieejams " );
                return false;
            }
        }

        public static Boolean WaitForPageFullyLoaded(IWebDriver driver, int timeoutInSecond)
        {
            Boolean pageLoadStatus = false;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSecond));
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            try
            {
                wait.Until(jsResult => js.ExecuteScript("return document.readyState").Equals("complete"));
                return true;
            } catch 
            {
                Console.WriteLine("ERROR: Lapa netika ielādeta pilnībā noradītājā laikā (Sekundes: " + timeoutInSecond + ").");
                return false;
            }
        }

        public static Boolean WaitUntilElementIsVisibleHrefLink(string HrefLink)
        {
            try
            { 
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(45));
                wait.Until(ExpectedConditions.ElementIsVisible(By.LinkText(HrefLink)));
                return true;
            } catch (Exception e)
            {
                Console.WriteLine("ERROR: Iestājas timeouts elementa <a> ar tekstu"+ HrefLink +" meklēšanā");
                Console.WriteLine("e");
                return false;
            }
        }

        //driver.FindElementsByLinkText("linktext")
        // Gaida, līdz atrod elementu (līdz tas parādās uz lapas)
        public static Boolean WaitUntilElementIsVisibleXPath(string XPathString, TimeSpan timeout)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, timeout);
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(XPathString)));
                return true;
            } catch (Exception e)
            {
                Console.WriteLine("ERROR: Iestājas timeouts gaidot DOM elementa ar Xpath" + XPathString + " pielasīšanu");
                Console.WriteLine("e");
                return false;
            }

        }

        public static Boolean KillChromeDriverProcess()
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName("chromedriver.exe"))
                {
                    proc.Kill();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Kluda pie chromedriver procesa aizversanas.");
                Console.WriteLine(e);
                return false;
            }
            return true;
        }


        // Pārbauda, vai elements/lauks ir redzams uz lapas
        public static bool ElementPresent(IWebElement element)
        {
            try
            {
                return element.Displayed;
            }
            catch
            {
                return false;
            }
        }

        // Pārbauda, vai elements/lauks ir redzams uz lapas - ja nē, tad met kļūdas ziņojumu
        public static void AssertElementPresent(IWebElement element, string elementDescription)
        {
            if (!ElementPresent(element))
                throw new Exception(String.Format("AssertElementPresent Failed: Could not find '{0}'", elementDescription));
        }

        // Notīra lauka saturu un ievada tekstu
        public static void ClearAndType(IWebElement element, string value)
        {
            try
            {
                System.Threading.Thread.Sleep(200);
                element.Clear();
                System.Threading.Thread.Sleep(200);
                element.SendKeys(value);
                System.Threading.Thread.Sleep(500);
            } catch (Exception e)
            {
                Console.WriteLine("ERROR: Kļūda - nevar aizpildīt teksta laukus.");
                Console.WriteLine(e);
            }
        }

        // Funkcija, kas izsauc automātisku klaviatūras pogas nospiešanu 
        // Pielietojuma piemērs -> PerformAction(OpenQA.Selenium.Keys.Enter);
        public static void PerformAction(string keysToSend)
        {
            System.Threading.Thread.Sleep(500);
            OpenQA.Selenium.Interactions.Actions builder = new OpenQA.Selenium.Interactions.Actions(driver);
            builder.SendKeys(keysToSend).Perform();
            System.Threading.Thread.Sleep(500);
        }

        public Boolean ElementHasClass(IWebElement element, string cssClass)
        {
            String classes = element.GetAttribute("class");
            foreach (String c in classes.Split(' '))
            {
                if (c.Equals(cssClass))
                {
                    return true;
                }
            }
            return false;
        }

        public static string SQLconnection(string connectionString, string sqlQuery)
        {
            MySqlConnector.MySqlConnection cnn;

            cnn = new MySqlConnector.MySqlConnection(connectionString);
            cnn.Open();

            MySqlCommand command;
            MySqlDataReader dataReader;
            String output = "";

            command = new MySqlCommand(sqlQuery, cnn);
            dataReader = command.ExecuteReader();
            int count = dataReader.FieldCount;
            while (dataReader.Read())
            {
                String dataReaderCurrentRow = "";
                for (int i = 0; i < count; i++)
                {
                    dataReaderCurrentRow += dataReader.GetValue(i).ToString();
                    if (i + 1 < count)
                    {
                        dataReaderCurrentRow += "###";
                    }
                }
                output += dataReaderCurrentRow;
                output += "@@@";
            }
            cnn.Close();
            //System.Console.WriteLine(Output);
            return output;
        }
    }
}
