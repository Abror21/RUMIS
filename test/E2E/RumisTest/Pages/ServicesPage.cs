using OpenQA.Selenium;
using RumisTest.Common;
using System;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using System.Threading;
using System.Configuration;
using System.Drawing;
using System.Collections.Generic;

namespace RumisTest.Pages
{
    class ServicesPage : BaseRunner
    {
        public static void clickToElement(String XPath, String errorMessage)
        {
            try
            {
                if (WaitUntilElementIsVisibleXPath(XPath, new TimeSpan(0, 0, 60)) == false)
                {
                    Assert.Fail("ERROR: Iestājies timeouts meklējot DOM elementu " + errorMessage);
                }
                System.Threading.Thread.Sleep(500);
                WaitWhileCurtainIsOn(20);
                driver.FindElement(By.XPath(XPath)).Click();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Neizdevās uzspiest uz " + errorMessage);
                Console.WriteLine("##vso[task.logissue type=error;]ERROR: Neizdevās uzspiest uz " + errorMessage);
                Console.WriteLine(e);
                Assert.Fail("Neizdevās uzspiest uz " + errorMessage);
            }
        }

        public static void fillElementWithText(String XPath, String textToElement, String errorMessage)
        {
            try
            {
                if (WaitUntilElementIsVisibleXPath(XPath, new TimeSpan(0, 0, 60)) == false)
                {
                    Assert.Fail("ERROR: Iestājies timeouts meklējot DOM elementu " + errorMessage);
                }
                System.Threading.Thread.Sleep(500);
                WaitWhileCurtainIsOn(20);
                driver.FindElement(By.XPath(XPath)).Clear();
                driver.FindElement(By.XPath(XPath)).SendKeys(textToElement);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Neizdevās ievadīt datus laukam " + errorMessage);
                Console.WriteLine("##vso[task.logissue type=error;]ERROR: Neizdevās ievadīt datus laukam " + errorMessage);
                Console.WriteLine(e);
                Assert.Fail("Neizdevās ievadīt datus laukam " + errorMessage);
            }
        }

        public static void clearCreatedRecordsFromDBVecaksAizbildnis()
        {
            String sqlConnection = ConfigurationManager.ConnectionStrings["rumis"].ConnectionString;

            String sqlQuery1 = "select @id := id from Applications where SubmitterPersonId = " +
                "'0691572b-683e-11ee-8117-0242ac110004' and SubmitterTypeId = '1b2e256a-c313-4ee2-9bf7-08e878f6b58c' " +
                "order by ApplicationDate desc limit 1; " + 
                "delete from ApplicationSocialStatuses where ApplicationId = @id; " + 
                "delete from ApplicationAttachments where ApplicationId = @id; " + 
                "delete from Applications where id = @id;";
            try
            {
                SQLconnection(sqlConnection, sqlQuery1);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Kļūda darbojoties ar datu bāzi");
                Console.WriteLine(e);
                Assert.Fail("Kļūda darbojoties ar datu bāzi");
                driver.Quit();
            }
        }

        public static void clearCreatedRecordsFromDBIzglitibasIestade()
        {
            String sqlConnection = ConfigurationManager.ConnectionStrings["rumis"].ConnectionString;

            String sqlQuery1 = "select @id := id from Applications where SubmitterPersonId = " +
                "'0691572b-683e-11ee-8117-0242ac110004' and SubmitterTypeId = '01843867-28be-4431-9878-653fcc2417c9' " +
                "order by ApplicationDate desc limit 1; " +
                "delete from ApplicationSocialStatuses where ApplicationId = @id; " +
                "delete from ApplicationAttachments where ApplicationId = @id; " +
                "delete from Applications where id = @id;";
            try
            {
                SQLconnection(sqlConnection, sqlQuery1);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Kļūda darbojoties ar datu bāzi");
                Console.WriteLine(e);
                Assert.Fail("Kļūda darbojoties ar datu bāzi");
                driver.Quit();
            }
        }

        public static void clearCreatedRecordsFromDBIzglitojamais()
        {
            String sqlConnection = ConfigurationManager.ConnectionStrings["rumis"].ConnectionString;

            String sqlQuery1 = "select @id := id from Applications where SubmitterPersonId = " +
                "'0691572b-683e-11ee-8117-0242ac110004' and SubmitterTypeId = '54799304-bb82-4879-8dd6-6877c748fd28' " +
                "order by ApplicationDate desc limit 1; " +
                "delete from ApplicationSocialStatuses where ApplicationId = @id; " +
                "delete from ApplicationAttachments where ApplicationId = @id; " +
                "delete from Applications where id = @id;";
            try
            {
                SQLconnection(sqlConnection, sqlQuery1);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Kļūda darbojoties ar datu bāzi");
                Console.WriteLine(e);
                Assert.Fail("Kļūda darbojoties ar datu bāzi");
                driver.Quit();
            }
        }
    }
}
