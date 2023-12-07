using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RumisTest.Common;
using RumisTest.Pages;
using System.Linq;
using System.Drawing;
using System.Collections.ObjectModel;
using System.Diagnostics;
using RumisTest;
using System.Threading;
using OpenQA.Selenium;

namespace RumisTest
{
    #region StudyPlan
    [TestClass]
    public class createIzglitojamais : BaseRunner
    {
        String XPath;
        String textToElement;
        String errorMessage;
        String currentSolisDesc;
        String testCaseDesc = "Testa scenārijs: Pieteikumu izveide, kad laukā \"Resursa pieteicējs\" vērtība tiek norādīta \"Izglītojamais\" (createIzglitojamais.cs).";
        String AdditionalURL = "";

        [TestMethod]
        public void runTest()
        {
            W("INFO: Tiek izdzēsts pieteikums no datubāzes, kurš tiek izveidots šajā testa scenārijā, ja tāds eksistē.\n");
            ServicesPage.clearCreatedRecordsFromDBIzglitojamais();

            if (KillChromeDriverProcess())
            W(testCaseDesc + "\n");
            W("INFO: ChromeDriver process ir izdzēsts testa sakumā.\n");

            currentSolisDesc = "1.Solis (1): Atvērt interneta pārlūku Google Chrome.";
            W(currentSolisDesc);
            W("(1) Palaiž Chrome.\n");
            initializeChrome();

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(100);

            currentSolisDesc = "2.Solis (2): Pārlūkprogrammā ievadīt URL uz RUMIS vidi.";
            W(currentSolisDesc);
            W("(2) Atver RUMIS vidi.\n");
            errorMessage = "RUMIS vides galveno lapu. " + currentSolisDesc + " " + testCaseDesc;
            LoginPage.login(AdditionalURL, errorMessage);

            currentSolisDesc = "3.Solis (3-): Autorizācija (lietotājs: admin).";
            W(currentSolisDesc);
            W("(3) Ievada datus \"Lietotājvārds\" laukā.\n");
            XPath = "//input[@id='basic_username']";
            textToElement = "admin";
            errorMessage = "\"Lietotājs\" laukā. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.fillElementWithText(XPath, textToElement, errorMessage);

            W("(4) Ievada datus \"Parole\" laukā.\n");
            XPath = "//input[@id='basic_password']";
            textToElement = "4FrRa61H4mY72oIFUZ89N22ZaaHLpt8z";
            errorMessage = "\"Parole\" laukā. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.fillElementWithText(XPath, textToElement, errorMessage);

            W("(5) Uzspiež uz \"Pieslēgties\" pogu.\n");
            XPath = "//button[@type='submit']";
            errorMessage = "\"Pieslēgties\" pogu. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(6) Uzspiež uz \"Izvēlies profilu\" izvelnes lauku.\n");
            XPath = "//input[@id='profile']//parent::span";
            errorMessage = "\"Izvēlies profilu\" lauku. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(7) Uzspiež uz \"Ventspils Valsts 1. ģimnāzija\" izvelnes opciju.\n");
            XPath = "//div[text()='Ventspils Valsts 1. ģimnāzija']";
            errorMessage = "\"Ventspils Valsts 1. ģimnāzija\" izvelnes lauku. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(8) Uzspiež uz \"Turpināt\" pogu.\n");
            XPath = "//button[@type='submit']";
            errorMessage = "\"Turpināt\" pogu. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            currentSolisDesc = "4.Solis (9-10): Atvērt jauna pieteikuma formu.";
            W(currentSolisDesc);
            W("(9) Uzspiež uz \"Pieteikumi\" pogu navigācijā.\n");
            XPath = "//a[@href='/admin/applications']";
            errorMessage = "\"Pieteikumi\" pogu navigācijā. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(10) Uzspiež uz \"Izveidot\" pogu.\n");
            XPath = "//button[contains(., 'Izveidot')]";
            errorMessage = "\"Izveidot\" pogu. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            currentSolisDesc = "5.Solis (11-29): Aizpildīt un saglabāt jaunu pieteikumu atbilstoši scenārija pieprasījumam (Resursa pieteicējs: Vecāks/aizbildnis).";
            W(currentSolisDesc);
            W("(11) Uzspiež uz \"Resursa pieteicējs\" izvelnes lauka.\n");
            XPath = "//input[@id='resourceApplicant']//parent::span";
            errorMessage = "\"Resursa pieteicējs\" lauka. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(12) Uzspiež uz \"Izglītojamais\" opciju iepriekš atvērtajā izvēlnē.\n");
            XPath = "//div[text()='Izglītojamais']";
            errorMessage = "\"Izglītojamais\" opciju iepriekš atvērtajā izvēlnē. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(13) Ievada datus \"Izglītojamā personas kods\" laukā.\n");
            XPath = "//input[@id='eduPersonalCode']";
            textToElement = "00000000000";
            errorMessage = "\"Izglītojamā personas kods\" laukā, jauna pieteikuma izveides formā. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.fillElementWithText(XPath, textToElement, errorMessage);

            W("(14) Uzspiež uz \"Izgūt datus\" pogu.\n");
            XPath = "//button[contains(., 'Izgūt datus')]";
            errorMessage = "\"Izgūt datus\" pogu. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(15) Uzspiež uz \"Atbilst\" opciju izvelnei (Vai izglītojamais atbilst kādai sociālai atbalsta grupai?).\n");
            XPath = "//div[@id='socialStatus']//child::input[@value='1']//parent::span";
            errorMessage = "\"Atbilst\" opciju izvelnei (Vai izglītojamais atbilst kādai sociālai atbalsta grupai?). " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(16) Uzspiež uz \"Invaliditāte\" opciju izvelnei (Norādiet vismaz vienu sociālā atbalsta grupu).\n");
            XPath = "//div[@id='socialStatuses']//child::input[@value='5be19fd9-e3da-4e4e-8564-33a04c197be1']//parent::span";
            errorMessage = "\"Invaliditāte\" opciju izvelnei (Norādiet vismaz vienu sociālā atbalsta grupu). " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(17) Uzspiež jeb atzīmē ka lietotājs ir informēts utt.\n");
            XPath = "//input[@id='socialStatusApproved']//parent::span";
            errorMessage = "Uzspiež jeb atzīmē ka lietotājs ir informēts utt. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(18) Uzspiež uz \"Resursa veids\" izvelnes lauku.\n");
            XPath = "//input[@id='resourceType']//parent::span";
            errorMessage = "\"Resursa veids\" izvelnes lauku. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(19) Uzspiež uz \"Dators\" opciju iepriekš atvērtajā izvēlnē.\n");
            XPath = "//div[@title='Dators']";
            errorMessage = "\"Dators\" opciju iepriekš atvērtajā izvēlnē. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(20) Uzspiež uz \"Resursa paveids\" izvelnes lauku.\n");
            XPath = "//input[@id='resourceSubTypeId']//parent::span";
            errorMessage = "\"Resursa paveids\" izvelnes lauku. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(21) Uzspiež uz \"Windows portatīvais dators\" opciju iepriekš atvērtajā izvēlnē.\n");
            XPath = "//div[@id='665ec601-8204-429a-8f60-ee137ed8a68b']";
            errorMessage = "\"Chromebook portatīvais dators\" opciju iepriekš atvērtajā izvēlnē. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(22) Ievada datus \"E-pasta adrese\" laukā.\n");
            XPath = "//input[@id='email']";
            textToElement = "teksts@teksts.teksts";
            errorMessage = "\"E-pasta adrese\" laukā. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.fillElementWithText(XPath, textToElement, errorMessage);

            W("(23) Ievada datus \"Tālrunis\" laukā.\n");
            XPath = "//input[@id='phoneNumber']";
            textToElement = "+37122222222";
            errorMessage = "\"Tālrunis\" laukā. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.fillElementWithText(XPath, textToElement, errorMessage);

            W("(24) Ievada datus \"Dokumenta nr.\" laukā.\n");
            XPath = "//input[@id='documentNr']";
            textToElement = "AA12";
            errorMessage = "\"Dokumenta nr.\" laukā. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.fillElementWithText(XPath, textToElement, errorMessage);

            W("(25) Uzspiež uz \"Dokumenta datums\" izvelnes lauku.\n");
            XPath = "//input[@id='documentDate']//parent::div";
            errorMessage = "\"Dokumenta datums\" izvelnes lauku. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(26) Uzspiež jeb izvēlas šodienas datuma opciju iepriekš atvērtajā datuma izvēlnē.\n");
            XPath = "//td[contains(@class, 'today')]";
            errorMessage = "šodienas datuma opciju iepriekš atvērtajā datuma izvēlnē. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(27) Izvēlās .docx failu norādot tā atrašanas vietu (Atrašanas vieta ir iekš šā automātiskā testa projekta direktorijas)");
            IWebElement byXPath = driver.FindElement(By.XPath("//input[@id='documentFile']"));
            textToElement = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + @"\TestsRUMIS.docx";
            W("FullPath: " + textToElement);
            errorMessage = "Izvēlās .docx failu norādot tā atrašanas vietu";
            byXPath.SendKeys(textToElement);
            //ServicesPage.fillElementWithText(XPath, textToElement, errorMessage);

            W("(28) Uzspiež uz \"Saglabāt\" pogu.\n");
            XPath = "//button[@type='submit']";
            errorMessage = "\"Saglabāt\" pogu. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(29) Pārbauda vai tika veiksmīgi saglabāts izveidotais pieteikums.");
            if (WaitUntilElementIsVisibleXPath("//button[contains(., 'Izveidot')]", new TimeSpan(0, 0, 60)) == true)
            {
                W("Tika veiksmīgi saglabāts izveidotais pieteikums.\n");
            }
            else
            {
                W("Netika veiksmīgi saglabāts izveidotais pieteikums. " + currentSolisDesc + " " + testCaseDesc + "\n");
                W("##vso[task.logissue type=error;]ERROR: Netika veiksmīgi saglabāts izveidotais pieteikums. " + currentSolisDesc + " " + testCaseDesc + "\n");
                Assert.Fail("Netika veiksmīgi saglabāts izveidotais pieteikums. " + currentSolisDesc + " " + testCaseDesc + "\n");
            }

            currentSolisDesc = "6.Solis (30-32): Izrakstīties no RUMIS vides.";
            W(currentSolisDesc);
            W("(30) Uzspiež profila bildi lai atvērtu izvēlni.\n");
            XPath = "//button[@class='ant-dropdown-trigger !bg-transparent']";
            errorMessage = "profila bildi lai atvērtu izvēlni. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(31) Uzspiež uz \"Iziet\" opciju iepriekš atvērtajā izvēlnē.\n");
            XPath = "//button[.='Iziet']";
            errorMessage = "\"Iziet\" opciju iepriekš atvērtajā izvēlnē. " + currentSolisDesc + " " + testCaseDesc;
            ServicesPage.clickToElement(XPath, errorMessage);

            W("(32) Pārbauda vai tika veiksmīgi izpildīta atslēgšanās no RUMIS vides.");
            if (WaitUntilElementIsVisibleXPath("//form[@id='basic']", new TimeSpan(0, 0, 15)) == true)
            {
                W("Tika veiksmīgi veikta atslēgšanos no RUMIS vides.\n");
            }
            else
            {
                W("Netika veiksmīgi veikta atslēgšanos no RUMIS vides. " + currentSolisDesc + " " + testCaseDesc + "\n");
                W("##vso[task.logissue type=error;]ERROR: Netika veiksmīgi veikta atslēgšanos no RUMIS vides. " + currentSolisDesc + " " + testCaseDesc + "\n");
                Assert.Fail("Netika veiksmīgi veikta atslēgšanos no RUMIS vides. " + currentSolisDesc + " " + testCaseDesc + "\n");
            }
        }
        [TestCleanup]
        public void Cleanup()
        {
                driver.Quit();
                KillChromeDriverProcess();
                W("ChromeDriver process ir izdzēsts.");
        }
    }
    #endregion
}
