using Microsoft.EntityFrameworkCore;
using OwlApi.Controllers;
using OwlApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OwlApi
{
    public class Initializer
    {
        public static void Initialize(OwlApiContext context)
        {
            context.Database.EnsureCreated();

            var countries = seedCountriesAndCombinations(context);

            var languages = new List<AppLanguage>() {
                new AppLanguage() {name = "English", subdomain = "", localeId = "en-US", Country = countries?.VelikaBritanijainSevernaIrska},
                new AppLanguage() {name = "Slovenian", subdomain = "sl", localeId = "sl", Country = countries?.Slovenija},
            };

            var languagesInDb = context.AppLanguages.ToList();
            var languagesToAdd = new List<AppLanguage>();
            foreach (var language in languages)
            {
                var foundLanguage = languagesInDb.Where(l => l.subdomain.Equals(language.subdomain)).FirstOrDefault();
                if (foundLanguage == null)
                {
                    languagesToAdd.Add(language);
                }
            }

            context.AppLanguages.AddRange(languagesToAdd);

            languagesInDb.AddRange(languagesToAdd);

            var companiesWithNoReservationFields = context.Companies
              .Include(c => c.ReservationFields)
              .Where(c => c.ReservationFields.Count == 0)
              .ToList();

            foreach (var companyWithoutReservationFields in companiesWithNoReservationFields)
            {
                Console.WriteLine($"No reservation fields for company {companyWithoutReservationFields.RealmName}; adding them");
                var fields = ReservationField.GetDefaultReservationFields(languagesInDb, companyWithoutReservationFields, null, null);
                context.ReservationFields.AddRange(fields);
            }

            var warehousesWithNoReservationFields = context.Warehouses
              .Include(u => u.ReservationFields)
              .Where(u => u.ReservationFields.Count == 0)
              .ToList();

            foreach (var warehouseWithoutReservationFields in warehousesWithNoReservationFields)
            {
                Console.WriteLine($"No reservation fields for warehouse {warehouseWithoutReservationFields.Name}; adding them");
                var fields = ReservationField.GetDefaultReservationFields(languagesInDb, null, warehouseWithoutReservationFields, null);
                context.ReservationFields.AddRange(fields);
            }

            var doorsWithNoReservationFields = context.Doors
              .Include(u => u.ReservationFields)
              .Where(u => u.ReservationFields.Count == 0)
              .ToList();

            foreach (var doorWithNoReservationFields in doorsWithNoReservationFields)
            {
                Console.WriteLine($"No reservation fields for door {doorWithNoReservationFields.Name}; adding them");
                var fields = ReservationField.GetDefaultReservationFields(languagesInDb, null, null, doorWithNoReservationFields);
                context.ReservationFields.AddRange(fields);
            }

            var companiesForTemplates = context.Companies.ToList();
            foreach (var company in companiesForTemplates)
            {
                EmailTemplatesController.GeneratDefaultEmailTemplatesForCompany(context, company.Id);
            }

            var twWithoutBookableWeekdays = context.TimeWindows.Where(a => a.BookableWeekdays == null).ToList();
            foreach (var a in twWithoutBookableWeekdays)
            {
                a.BookableWeekdays = new List<int> { 0, 1, 2, 3, 4, 5, 6, };
                context.Update(a);
            }

            context.SaveChanges();
        }

        private class ImportantCountries
        {
            public Country Slovenija { get; set; }
            public Country VelikaBritanijainSevernaIrska { get; set; }
        }

        private static ImportantCountries seedCountriesAndCombinations(OwlApiContext context)
        {
            if (context.Countries.Any())
            {
                return null;   // DB has been seeded
            }

            Country Albanija = new Country { name = "Albanija", type = CountryType.Third };
            Country Andora = new Country { name = "Andora", type = CountryType.EU_EFTA };
            Country Armenija = new Country { name = "Armenija", type = CountryType.Third };
            Country Avstrija = new Country { name = "Avstrija", type = CountryType.EU_EFTA };
            Country Azerbajdzan = new Country { name = "Azerbajdžan", type = CountryType.Third };
            Country Belgija = new Country { name = "Belgija", type = CountryType.EU_EFTA };
            Country Belorusija = new Country { name = "Belorusija", type = CountryType.Third };
            Country Bih = new Country { name = "Bih", type = CountryType.Third };
            Country Bolgarija = new Country { name = "Bolgarija", type = CountryType.EU_EFTA };
            Country Ciper = new Country { name = "Ciper", type = CountryType.EU_EFTA };
            Country Ceska = new Country { name = "Češka", type = CountryType.EU_EFTA };
            Country CrnaGora = new Country { name = "Črna Gora", type = CountryType.Third };
            Country Danska = new Country { name = "Danska", type = CountryType.EU_EFTA };
            Country Estonija = new Country { name = "Estonija", type = CountryType.EU_EFTA };
            Country Finska = new Country { name = "Finska", type = CountryType.EU_EFTA };
            Country Francija = new Country { name = "Francija", type = CountryType.EU_EFTA };
            Country Gruzija = new Country { name = "Gruzija", type = CountryType.Third };
            Country Grcija = new Country { name = "Grčija", type = CountryType.EU_EFTA };
            Country Hrvaska = new Country { name = "Hrvaška", type = CountryType.EU_EFTA };
            Country Iran = new Country { name = "Iran", type = CountryType.Third };
            Country Irska = new Country { name = "Irska", type = CountryType.EU_EFTA };
            Country Islandija = new Country { name = "Islandija", type = CountryType.EU_EFTA };
            Country Italija = new Country { name = "Italija", type = CountryType.EU_EFTA };
            Country Kazahstan = new Country { name = "Kazahstan", type = CountryType.Third };
            Country Kirgizistan = new Country { name = "Kirgizistan", type = CountryType.Third };
            Country Kosovo = new Country { name = "Kosovo", type = CountryType.Third };
            Country Latvija = new Country { name = "Latvija", type = CountryType.EU_EFTA };
            Country Liechtenstein = new Country { name = "Liechtenstein", type = CountryType.EU_EFTA };
            Country Litva = new Country { name = "Litva", type = CountryType.EU_EFTA };
            Country Luksemburg = new Country { name = "Luksemburg", type = CountryType.EU_EFTA };
            Country Madzarska = new Country { name = "Madžarska", type = CountryType.EU_EFTA };
            Country Makedonija = new Country { name = "Makedonija", type = CountryType.Third };
            Country Malta = new Country { name = "Malta", type = CountryType.EU_EFTA };
            Country Maroko = new Country { name = "Maroko", type = CountryType.Third };
            Country Moldavija = new Country { name = "Moldavija", type = CountryType.Third };
            Country Nemcija = new Country { name = "Nemčija", type = CountryType.EU_EFTA };
            Country Nizozemska = new Country { name = "Nizozemska", type = CountryType.EU_EFTA };
            Country Norveska = new Country { name = "Norveška", type = CountryType.EU_EFTA };
            Country Poljska = new Country { name = "Poljska", type = CountryType.EU_EFTA };
            Country Portugalska = new Country { name = "Portugalska", type = CountryType.EU_EFTA };
            Country Romunija = new Country { name = "Romunija", type = CountryType.EU_EFTA };
            Country RuskaFederacija = new Country { name = "Ruska Federacija", type = CountryType.Third };
            Country SanMarino = new Country { name = "San Marino", type = CountryType.EU_EFTA };
            Country Slovaska = new Country { name = "Slovaška", type = CountryType.EU_EFTA };
            Country Slovenija = new Country { name = "Slovenija", type = CountryType.EU_EFTA };
            Country Srbija = new Country { name = "Srbija", type = CountryType.Third };
            Country Spanija = new Country { name = "Španija", type = CountryType.EU_EFTA };
            Country Svedska = new Country { name = "Švedska", type = CountryType.EU_EFTA };
            Country Svica = new Country { name = "Švica", type = CountryType.EU_EFTA };
            Country Tadzikistan = new Country { name = "Tadžikistan", type = CountryType.Third };
            Country Turkmenistan = new Country { name = "Turkmenistan", type = CountryType.Third };
            Country Turcija = new Country { name = "Turčija", type = CountryType.Third };
            Country Ukrajina = new Country { name = "Ukrajina", type = CountryType.Third };
            Country Uzbekistan = new Country { name = "Uzbekistan", type = CountryType.Third };
            Country VelikaBritanijainSevernaIrska = new Country { name = "Velika Britanija in Severna Irska", type = CountryType.EU_EFTA };

            var Countries = new Country[] {
        Albanija,
        Andora,
        Armenija,
        Avstrija,
        Azerbajdzan,
        Belgija,
        Belorusija,
        Bih,
        Bolgarija,
        Ciper,
        Ceska,
        CrnaGora,
        Danska,
        Estonija,
        Finska,
        Francija,
        Gruzija,
        Grcija,
        Hrvaska,
        Iran,
        Irska,
        Islandija,
        Italija,
        Kazahstan,
        Kirgizistan,
        Kosovo,
        Latvija,
        Liechtenstein,
        Litva,
        Luksemburg,
        Madzarska,
        Makedonija,
        Malta,
        Maroko,
        Moldavija,
        Nemcija,
        Nizozemska,
        Norveska,
        Poljska,
        Portugalska,
        Romunija,
        RuskaFederacija,
        SanMarino,
        Slovaska,
        Slovenija,
        Srbija,
        Spanija,
        Svedska,
        Svica,
        Tadzikistan,
        Turkmenistan,
        Turcija,
        Ukrajina,
        Uzbekistan,
        VelikaBritanijainSevernaIrska
      };

            foreach (Country c in Countries)
            {
                context.Countries.Add(c);
            }

            return new ImportantCountries()
            {
                Slovenija = Slovenija,
                VelikaBritanijainSevernaIrska = VelikaBritanijainSevernaIrska
            };
        }
    }
}
