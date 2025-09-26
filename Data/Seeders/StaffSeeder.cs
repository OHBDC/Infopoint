using InfoPoint.Models;
using Microsoft.EntityFrameworkCore;

namespace InfoPoint.Data.Seeders
{
    public static class StaffSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Staff.AnyAsync())
                return; // Database has been seeded

            var staffMembers = new List<Staff>
            {
                // Senior Management Team
                new Staff
                {
                    FirstName = "Sarah", LastName = "Johnson",
                    Email = "sarah.johnson@g.bdc.ac.uk", StaffReference = "12345001",
                    Area = "Senior Management", ManagerEmail = null,
                    JobTitle = "Principal", IsActive = true
                },
                new Staff
                {
                    FirstName = "Michael", LastName = "Davies",
                    Email = "michael.davies@g.bdc.ac.uk", StaffReference = "12345002",
                    Area = "Senior Management", ManagerEmail = "sarah.johnson@g.bdc.ac.uk",
                    JobTitle = "Vice Principal Academic", IsActive = true
                },
                new Staff
                {
                    FirstName = "Emma", LastName = "Wilson",
                    Email = "emma.wilson@g.bdc.ac.uk", StaffReference = "12345003",
                    Area = "Senior Management", ManagerEmail = "sarah.johnson@g.bdc.ac.uk",
                    JobTitle = "Vice Principal Finance & Resources", IsActive = true
                },

                // Computing & IT Department
                new Staff
                {
                    FirstName = "James", LastName = "Anderson",
                    Email = "james.anderson@g.bdc.ac.uk", StaffReference = "12345101",
                    Area = "Computing & IT", ManagerEmail = "michael.davies@g.bdc.ac.uk",
                    JobTitle = "Head of Computing & IT", IsActive = true
                },
                new Staff
                {
                    FirstName = "Rachel", LastName = "Thompson",
                    Email = "rachel.thompson@g.bdc.ac.uk", StaffReference = "12345102",
                    Area = "Computing & IT", ManagerEmail = "james.anderson@g.bdc.ac.uk",
                    JobTitle = "Senior Lecturer Computing", IsActive = true
                },
                new Staff
                {
                    FirstName = "David", LastName = "Brown",
                    Email = "david.brown@g.bdc.ac.uk", StaffReference = "12345103",
                    Area = "Computing & IT", ManagerEmail = "james.anderson@g.bdc.ac.uk",
                    JobTitle = "Lecturer Software Development", IsActive = true
                },
                new Staff
                {
                    FirstName = "Lisa", LastName = "Garcia",
                    Email = "lisa.garcia@g.bdc.ac.uk", StaffReference = "12345104",
                    Area = "Computing & IT", ManagerEmail = "james.anderson@g.bdc.ac.uk",
                    JobTitle = "IT Support Technician", IsActive = true
                },

                // Engineering Department
                new Staff
                {
                    FirstName = "Robert", LastName = "Miller",
                    Email = "robert.miller@g.bdc.ac.uk", StaffReference = "12345201",
                    Area = "Engineering", ManagerEmail = "michael.davies@g.bdc.ac.uk",
                    JobTitle = "Head of Engineering", IsActive = true
                },
                new Staff
                {
                    FirstName = "Jennifer", LastName = "Taylor",
                    Email = "jennifer.taylor@g.bdc.ac.uk", StaffReference = "12345202",
                    Area = "Engineering", ManagerEmail = "robert.miller@g.bdc.ac.uk",
                    JobTitle = "Senior Lecturer Mechanical Engineering", IsActive = true
                },
                new Staff
                {
                    FirstName = "Andrew", LastName = "Williams",
                    Email = "andrew.williams@g.bdc.ac.uk", StaffReference = "12345203",
                    Area = "Engineering", ManagerEmail = "robert.miller@g.bdc.ac.uk",
                    JobTitle = "Lecturer Electrical Engineering", IsActive = true
                },

                // Business Studies Department
                new Staff
                {
                    FirstName = "Catherine", LastName = "Jones",
                    Email = "catherine.jones@g.bdc.ac.uk", StaffReference = "12345301",
                    Area = "Business Studies", ManagerEmail = "michael.davies@g.bdc.ac.uk",
                    JobTitle = "Head of Business Studies", IsActive = true
                },
                new Staff
                {
                    FirstName = "Mark", LastName = "Evans",
                    Email = "mark.evans@g.bdc.ac.uk", StaffReference = "12345302",
                    Area = "Business Studies", ManagerEmail = "catherine.jones@g.bdc.ac.uk",
                    JobTitle = "Senior Lecturer Business Administration", IsActive = true
                },
                new Staff
                {
                    FirstName = "Helen", LastName = "Robinson",
                    Email = "helen.robinson@g.bdc.ac.uk", StaffReference = "12345303",
                    Area = "Business Studies", ManagerEmail = "catherine.jones@g.bdc.ac.uk",
                    JobTitle = "Lecturer Marketing", IsActive = true
                },

                // Health & Social Care Department
                new Staff
                {
                    FirstName = "Susan", LastName = "Lewis",
                    Email = "susan.lewis@g.bdc.ac.uk", StaffReference = "12345401",
                    Area = "Health & Social Care", ManagerEmail = "michael.davies@g.bdc.ac.uk",
                    JobTitle = "Head of Health & Social Care", IsActive = true
                },
                new Staff
                {
                    FirstName = "Paul", LastName = "Harris",
                    Email = "paul.harris@g.bdc.ac.uk", StaffReference = "12345402",
                    Area = "Health & Social Care", ManagerEmail = "susan.lewis@g.bdc.ac.uk",
                    JobTitle = "Senior Lecturer Nursing", IsActive = true
                },
                new Staff
                {
                    FirstName = "Amanda", LastName = "Clark",
                    Email = "amanda.clark@g.bdc.ac.uk", StaffReference = "12345403",
                    Area = "Health & Social Care", ManagerEmail = "susan.lewis@g.bdc.ac.uk",
                    JobTitle = "Lecturer Health & Social Care", IsActive = true
                },

                // Student Services
                new Staff
                {
                    FirstName = "Kevin", LastName = "White",
                    Email = "kevin.white@g.bdc.ac.uk", StaffReference = "12345501",
                    Area = "Student Services", ManagerEmail = "emma.wilson@g.bdc.ac.uk",
                    JobTitle = "Head of Student Services", IsActive = true
                },
                new Staff
                {
                    FirstName = "Julie", LastName = "Green",
                    Email = "julie.green@g.bdc.ac.uk", StaffReference = "12345502",
                    Area = "Student Services", ManagerEmail = "kevin.white@g.bdc.ac.uk",
                    JobTitle = "Student Support Advisor", IsActive = true
                },
                new Staff
                {
                    FirstName = "Gary", LastName = "Turner",
                    Email = "gary.turner@g.bdc.ac.uk", StaffReference = "12345503",
                    Area = "Student Services", ManagerEmail = "kevin.white@g.bdc.ac.uk",
                    JobTitle = "Admissions Officer", IsActive = true
                },

                // HR Department
                new Staff
                {
                    FirstName = "Michelle", LastName = "Parker",
                    Email = "michelle.parker@g.bdc.ac.uk", StaffReference = "12345601",
                    Area = "Human Resources", ManagerEmail = "emma.wilson@g.bdc.ac.uk",
                    JobTitle = "Head of Human Resources", IsActive = true
                },
                new Staff
                {
                    FirstName = "Simon", LastName = "Cooper",
                    Email = "simon.cooper@g.bdc.ac.uk", StaffReference = "12345602",
                    Area = "Human Resources", ManagerEmail = "michelle.parker@g.bdc.ac.uk",
                    JobTitle = "HR Advisor", IsActive = true
                },

                // Finance Department
                new Staff
                {
                    FirstName = "Diane", LastName = "Bailey",
                    Email = "diane.bailey@g.bdc.ac.uk", StaffReference = "12345701",
                    Area = "Finance", ManagerEmail = "emma.wilson@g.bdc.ac.uk",
                    JobTitle = "Finance Manager", IsActive = true
                },
                new Staff
                {
                    FirstName = "Tony", LastName = "Reed",
                    Email = "tony.reed@g.bdc.ac.uk", StaffReference = "12345702",
                    Area = "Finance", ManagerEmail = "diane.bailey@g.bdc.ac.uk",
                    JobTitle = "Finance Assistant", IsActive = true
                },

                // Maintenance & Facilities
                new Staff
                {
                    FirstName = "Steve", LastName = "Hughes",
                    Email = "steve.hughes@g.bdc.ac.uk", StaffReference = "12345801",
                    Area = "Facilities", ManagerEmail = "emma.wilson@g.bdc.ac.uk",
                    JobTitle = "Facilities Manager", IsActive = true
                },
                new Staff
                {
                    FirstName = "Carol", LastName = "Morris",
                    Email = "carol.morris@g.bdc.ac.uk", StaffReference = "12345802",
                    Area = "Facilities", ManagerEmail = "steve.hughes@g.bdc.ac.uk",
                    JobTitle = "Cleaning Supervisor", IsActive = true
                },

                // Add Oliver Hill for testing - make him a manager
                new Staff
                {
                    FirstName = "Oliver", LastName = "Hill",
                    Email = "oliver.hill@g.bdc.ac.uk", StaffReference = "12345105",
                    Area = "Computing & IT", ManagerEmail = "james.anderson@g.bdc.ac.uk",
                    JobTitle = "Senior Software Developer", IsActive = true
                },
                
                // Add some staff who report to Oliver
                new Staff
                {
                    FirstName = "John", LastName = "Smith",
                    Email = "john.smith@g.bdc.ac.uk", StaffReference = "12345106",
                    Area = "Computing & IT", ManagerEmail = "oliver.hill@g.bdc.ac.uk",
                    JobTitle = "Junior Software Developer", IsActive = true
                },
                new Staff
                {
                    FirstName = "Emily", LastName = "Johnson",
                    Email = "emily.johnson@g.bdc.ac.uk", StaffReference = "12345107",
                    Area = "Computing & IT", ManagerEmail = "oliver.hill@g.bdc.ac.uk",
                    JobTitle = "Web Developer", IsActive = true
                },
                new Staff
                {
                    FirstName = "Alex", LastName = "Davis",
                    Email = "alex.davis@g.bdc.ac.uk", StaffReference = "12345108",
                    Area = "Computing & IT", ManagerEmail = "oliver.hill@g.bdc.ac.uk",
                    JobTitle = "Software Tester", IsActive = true
                }
            };

            context.Staff.AddRange(staffMembers);
            await context.SaveChangesAsync();
        }
    }
}