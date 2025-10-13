using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InfoPoint.Data;
using InfoPoint.Models;
using InfoPoint.Authorization;
using System.Globalization;

namespace InfoPoint.Controllers
{
    [HRAuthorize]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UliveDbContext _uliveContext;

        public HRController(ApplicationDbContext context, UliveDbContext uliveContext)
        {
            _context = context;
            _uliveContext = uliveContext;
        }

        // GET: HR
        public async Task<IActionResult> Index()
        {
            // Load ALL staff data (no eager loading - StaffAD is ignored in DbContext)
            var staff = await _context.StaffHR
                .OrderBy(s => s.FullName)
                .ToListAsync();

            // Get all employee numbers (both for staff and managers) to load AD data in bulk
            var allEmployeeNumbers = staff.Select(s => s.EmployeeNumber).ToList();
            var managerEmployeeNumbers = staff
                .Where(s => s.ManagerEmployeeNumber.HasValue)
                .Select(s => s.ManagerEmployeeNumber!.Value)
                .Where(id => !allEmployeeNumbers.Contains(id)) // Only get managers not already in the list
                .ToList();

            var allNeededEmployeeIds = allEmployeeNumbers.Concat(managerEmployeeNumbers).Distinct().ToList();

            // Load ALL AD data (staff + managers) in a SINGLE query
            var allStaffAD = await _context.Staff
                .Where(ad => allNeededEmployeeIds.Contains(ad.Id))
                .ToDictionaryAsync(ad => ad.Id, ad => ad);

            // Load ALL card tokens from Ulive database (filter in memory to avoid SQL translation issues)
            // The CardToken table is relatively small, so loading all is acceptable
            var allCardTokens = await _uliveContext.CardTokens.ToListAsync();

            // Create lookup sets for faster filtering in memory
            var decimalReferencesSet = allEmployeeNumbers.Select(n => n.ToString()).ToHashSet();
            var hexReferencesSet = allEmployeeNumbers.Select(n => n.ToString("X").ToUpper()).ToHashSet();

            // Filter card tokens in memory to only those belonging to our staff
            allCardTokens = allCardTokens
                .Where(ct => decimalReferencesSet.Contains(ct.Reference) ||
                            hexReferencesSet.Contains(ct.Reference?.ToUpper() ?? ""))
                .ToList();

            // Group card tokens by employee number for quick lookup
            var cardTokensByEmployee = new Dictionary<int, List<CardToken>>();
            foreach (var empNo in allEmployeeNumbers)
            {
                var decimalRef = empNo.ToString();
                var hexRef = empNo.ToString("X");
                var tokens = allCardTokens
                    .Where(ct => ct.Reference == decimalRef || ct.Reference == hexRef)
                    .ToList();
                if (tokens.Any())
                {
                    cardTokensByEmployee[empNo] = tokens;
                }
            }

            // Assign StaffAD, ManagerAD, and card tokens to each staff member
            foreach (var s in staff)
            {
                // Assign staff AD data if available
                if (allStaffAD.ContainsKey(s.EmployeeNumber))
                {
                    s.StaffAD = allStaffAD[s.EmployeeNumber];
                }

                // Assign manager AD data if available
                if (s.ManagerEmployeeNumber.HasValue && allStaffAD.ContainsKey(s.ManagerEmployeeNumber.Value))
                {
                    s.ManagerAD = allStaffAD[s.ManagerEmployeeNumber.Value];
                }

                // Assign card tokens if available
                if (cardTokensByEmployee.ContainsKey(s.EmployeeNumber))
                {
                    s.CardTokens = cardTokensByEmployee[s.EmployeeNumber];
                }
            }

            // Find AD staff not in HR list (active only)
            var hrEmployeeNumbers = staff.Select(s => s.EmployeeNumber).ToHashSet();
            var adOnlyStaff = await _context.Staff
                .Where(ad => ad.Active == 1 && !hrEmployeeNumbers.Contains(ad.Id))
                .OrderBy(ad => ad.Firstname)
                .ThenBy(ad => ad.Surname)
                .ToListAsync();

            ViewBag.ADOnlyStaff = adOnlyStaff;

            // Calculate metrics (based on all staff)
            var totalStaff = staff.Count;
            var activeStaff = staff.Count(s => s.IsActive);
            var inactiveStaff = staff.Count(s => !s.IsActive);
            var notInAD = staff.Count(s => s.StaffAD == null);
            var inADButInactive = staff.Count(s => s.StaffAD != null && !s.StaffAD.IsActive);
            var managerNotInAD = staff.Count(s => s.ManagerEmployeeNumber.HasValue && s.ManagerAD == null);
            var managerInactive = staff.Count(s => s.ManagerAD != null && !s.ManagerAD.IsActive);
            var noManager = staff.Count(s => !s.ManagerEmployeeNumber.HasValue || string.IsNullOrEmpty(s.ManagerName));
            var totalProblems = staff.Count(s =>
                s.StaffAD == null ||
                (s.StaffAD != null && !s.StaffAD.IsActive) ||
                (s.ManagerEmployeeNumber.HasValue && s.ManagerAD == null) ||
                (s.ManagerAD != null && !s.ManagerAD.IsActive) ||
                !s.ManagerEmployeeNumber.HasValue ||
                string.IsNullOrEmpty(s.ManagerName)
            );

            // Pass metrics to view
            ViewBag.TotalStaff = totalStaff;
            ViewBag.ActiveStaff = activeStaff;
            ViewBag.InactiveStaff = inactiveStaff;
            ViewBag.NotInAD = notInAD;
            ViewBag.InADButInactive = inADButInactive;
            ViewBag.ManagerNotInAD = managerNotInAD;
            ViewBag.ManagerInactive = managerInactive;
            ViewBag.NoManager = noManager;
            ViewBag.TotalProblems = totalProblems;
            ViewBag.ADOnlyCount = adOnlyStaff.Count;

            // Get distinct values for filters
            ViewBag.ContractTypes = await _context.StaffHR
                .Where(s => !string.IsNullOrEmpty(s.ContractType))
                .Select(s => s.ContractType)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Get all unique departments from ALL 5 hierarchy levels
            var allDepartments = new HashSet<string>();
            foreach (var s in staff)
            {
                if (!string.IsNullOrEmpty(s.HierarchyLevel1)) allDepartments.Add(s.HierarchyLevel1);
                if (!string.IsNullOrEmpty(s.HierarchyLevel2)) allDepartments.Add(s.HierarchyLevel2);
                if (!string.IsNullOrEmpty(s.HierarchyLevel3)) allDepartments.Add(s.HierarchyLevel3);
                if (!string.IsNullOrEmpty(s.HierarchyLevel4)) allDepartments.Add(s.HierarchyLevel4);
                if (!string.IsNullOrEmpty(s.HierarchyLevel5)) allDepartments.Add(s.HierarchyLevel5);
            }
            ViewBag.Departments = allDepartments.OrderBy(d => d).ToList();

            return View(staff);
        }

        // GET: HR/DepartmentAnalysis
        [HRAuthorize]
        public async Task<IActionResult> DepartmentAnalysis()
        {
            // Filter for Lecturers by contract type
            var staff = await _context.StaffHR
                .Where(s => s.ContractType.ToLower().Contains("lecturer"))
                .ToListAsync();

            ViewBag.TotalLecturers = staff.Count;

            // Collect all departments with their counts and hierarchy level
            var departmentData = new List<DepartmentInfo>();

            foreach (var s in staff)
            {
                if (!string.IsNullOrEmpty(s.HierarchyLevel1))
                    AddDepartment(departmentData, s.HierarchyLevel1, 1, s.EmployeeNumber);
                if (!string.IsNullOrEmpty(s.HierarchyLevel2))
                    AddDepartment(departmentData, s.HierarchyLevel2, 2, s.EmployeeNumber);
                if (!string.IsNullOrEmpty(s.HierarchyLevel3))
                    AddDepartment(departmentData, s.HierarchyLevel3, 3, s.EmployeeNumber);
                if (!string.IsNullOrEmpty(s.HierarchyLevel4))
                    AddDepartment(departmentData, s.HierarchyLevel4, 4, s.EmployeeNumber);
                if (!string.IsNullOrEmpty(s.HierarchyLevel5))
                    AddDepartment(departmentData, s.HierarchyLevel5, 5, s.EmployeeNumber);
            }

            // Group similar departments
            var similarGroups = FindSimilarDepartments(departmentData);

            return View(similarGroups);
        }

        private void AddDepartment(List<DepartmentInfo> list, string name, int level, int employeeNumber)
        {
            var existing = list.FirstOrDefault(d => d.Name == name && d.Level == level);
            if (existing != null)
            {
                existing.Count++;
                existing.EmployeeNumbers.Add(employeeNumber);
            }
            else
            {
                list.Add(new DepartmentInfo
                {
                    Name = name,
                    Level = level,
                    Count = 1,
                    EmployeeNumbers = new List<int> { employeeNumber }
                });
            }
        }

        private List<SimilarDepartmentGroup> FindSimilarDepartments(List<DepartmentInfo> departments)
        {
            var groups = new List<SimilarDepartmentGroup>();
            var processed = new HashSet<string>();

            foreach (var dept in departments.OrderByDescending(d => d.Count))
            {
                if (processed.Contains(dept.Name)) continue;

                var similar = new List<DepartmentInfo> { dept };
                processed.Add(dept.Name);

                // Find similar departments
                foreach (var other in departments)
                {
                    if (processed.Contains(other.Name)) continue;
                    if (AreSimilar(dept.Name, other.Name))
                    {
                        similar.Add(other);
                        processed.Add(other.Name);
                    }
                }

                // Only add groups with more than one similar department
                if (similar.Count > 1 || HasIssues(dept.Name))
                {
                    groups.Add(new SimilarDepartmentGroup
                    {
                        Departments = similar.OrderByDescending(d => d.Count).ToList()
                    });
                }
            }

            return groups.OrderByDescending(g => g.TotalCount).ToList();
        }

        private bool AreSimilar(string name1, string name2)
        {
            var n1 = name1.ToLower().Trim();
            var n2 = name2.ToLower().Trim();

            // Exact match
            if (n1 == n2) return true;

            // Check for common variations
            // Remove special characters for comparison
            var clean1 = new string(n1.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
            var clean2 = new string(n2.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());

            if (clean1 == clean2) return true;

            // Check if one contains the other (for abbreviations)
            if (n1.Contains(n2) || n2.Contains(n1)) return true;

            // Check for similar length and characters (basic fuzzy matching)
            if (Math.Abs(n1.Length - n2.Length) <= 3)
            {
                var similarity = CalculateSimilarity(n1, n2);
                if (similarity > 0.85) return true;
            }

            return false;
        }

        private bool HasIssues(string name)
        {
            // Check for potential issues like trailing spaces, special chars, inconsistent casing
            return name != name.Trim() ||
                   name.Contains("  ") ||
                   name.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c) && c != '&' && c != '-' && c != '/');
        }

        private double CalculateSimilarity(string s1, string s2)
        {
            var longer = s1.Length > s2.Length ? s1 : s2;
            var shorter = s1.Length > s2.Length ? s2 : s1;

            if (longer.Length == 0) return 1.0;

            var editDistance = LevenshteinDistance(longer, shorter);
            return (longer.Length - editDistance) / (double)longer.Length;
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            var matrix = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s2[j - 1] == s1[i - 1]) ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[s1.Length, s2.Length];
        }

        // GET: HR/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staffHR = await _context.StaffHR
                .FirstOrDefaultAsync(m => m.Id == id);

            if (staffHR == null)
            {
                return NotFound();
            }

            // Manually load StaffAD data
            staffHR.StaffAD = await _context.Staff.FirstOrDefaultAsync(ad => ad.Id == staffHR.EmployeeNumber);

            // Load card tokens from Ulive database
            // Try both decimal string and hex string formats
            var decimalReference = staffHR.EmployeeNumber.ToString();
            var hexReference = staffHR.EmployeeNumber.ToString("X");
            staffHR.CardTokens = await _uliveContext.CardTokens
                .Where(ct => ct.Reference == decimalReference || ct.Reference == hexReference)
                .ToListAsync();

            return View(staffHR);
        }

        // GET: HR/Create
        public async Task<IActionResult> Create()
        {
            // Load all staff for manager dropdown
            var allStaff = await _context.StaffHR
                .Where(s => s.IsActive)
                .OrderBy(s => s.FullName)
                .Select(s => new { s.EmployeeNumber, s.FullName })
                .ToListAsync();

            ViewBag.AllStaff = allStaff;

            return View();
        }

        // POST: HR/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeNumber,FullName,HierarchyLevel1,HierarchyLevel2,HierarchyLevel3,HierarchyLevel4,HierarchyLevel5,JobTitle,ContractType,ManagerEmployeeNumber,ManagerName,IsActive")] StaffHR staffHR)
        {
            if (ModelState.IsValid)
            {
                // Check if employee number already exists
                var exists = await _context.StaffHR.AnyAsync(s => s.EmployeeNumber == staffHR.EmployeeNumber);
                if (exists)
                {
                    ModelState.AddModelError("EmployeeNumber", "An employee with this number already exists.");
                    return View(staffHR);
                }

                staffHR.CreatedDate = DateTime.UtcNow;
                _context.Add(staffHR);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Staff member {staffHR.FullName} has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(staffHR);
        }

        // GET: HR/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staffHR = await _context.StaffHR.FindAsync(id);
            if (staffHR == null)
            {
                return NotFound();
            }

            // Load all staff for manager dropdown
            var allStaff = await _context.StaffHR
                .Where(s => s.IsActive)
                .OrderBy(s => s.FullName)
                .Select(s => new { s.EmployeeNumber, s.FullName })
                .ToListAsync();

            ViewBag.AllStaff = allStaff;

            return View(staffHR);
        }

        // POST: HR/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmployeeNumber,FullName,HierarchyLevel1,HierarchyLevel2,HierarchyLevel3,HierarchyLevel4,HierarchyLevel5,JobTitle,ContractType,ManagerEmployeeNumber,ManagerName,IsActive,CreatedDate")] StaffHR staffHR)
        {
            if (id != staffHR.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    staffHR.LastUpdated = DateTime.UtcNow;
                    _context.Update(staffHR);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Staff member {staffHR.FullName} has been updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StaffHRExists(staffHR.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(staffHR);
        }

        // GET: HR/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staffHR = await _context.StaffHR
                .FirstOrDefaultAsync(m => m.Id == id);

            if (staffHR == null)
            {
                return NotFound();
            }

            // Manually load StaffAD data
            staffHR.StaffAD = await _context.Staff.FirstOrDefaultAsync(ad => ad.Id == staffHR.EmployeeNumber);

            // Load card tokens from Ulive database
            // Try both decimal string and hex string formats
            var decimalReference = staffHR.EmployeeNumber.ToString();
            var hexReference = staffHR.EmployeeNumber.ToString("X");
            staffHR.CardTokens = await _uliveContext.CardTokens
                .Where(ct => ct.Reference == decimalReference || ct.Reference == hexReference)
                .ToListAsync();

            return View(staffHR);
        }

        // POST: HR/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var staffHR = await _context.StaffHR.FindAsync(id);
            if (staffHR != null)
            {
                _context.StaffHR.Remove(staffHR);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Staff member {staffHR.FullName} has been deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: HR/QuickAddFromAD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickAddFromAD(int employeeNumber)
        {
            try
            {
                // Check if staff already exists in HR
                var exists = await _context.StaffHR.AnyAsync(s => s.EmployeeNumber == employeeNumber);
                if (exists)
                {
                    TempData["ErrorMessage"] = "This staff member already exists in the HR list.";
                    return RedirectToAction(nameof(Index));
                }

                // Get staff from AD
                var adStaff = await _context.Staff.FirstOrDefaultAsync(s => s.Id == employeeNumber);
                if (adStaff == null)
                {
                    TempData["ErrorMessage"] = "Staff member not found in Active Directory.";
                    return RedirectToAction(nameof(Index));
                }

                // Create new HR record with AD data
                var newStaffHR = new StaffHR
                {
                    EmployeeNumber = adStaff.Id,
                    FullName = adStaff.FullName,
                    JobTitle = adStaff.JobTitle ?? "",
                    ContractType = "", // To be filled in by user
                    HierarchyLevel1 = adStaff.Department,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.StaffHR.Add(newStaffHR);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Staff member {adStaff.FullName} has been added to the HR list. Please update their contract type and hierarchy information.";
                return RedirectToAction(nameof(Edit), new { id = newStaffHR.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adding staff member: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: HR/BulkImport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkImport(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                List<StaffHRImportModel> staffList = new List<StaffHRImportModel>();

                // Handle CSV files
                if (fileExtension == ".csv")
                {
                    using var reader = new StreamReader(file.OpenReadStream());

                    // Skip header row
                    var headerLine = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(headerLine))
                    {
                        TempData["ErrorMessage"] = "CSV file is empty.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Read data rows
                    string? line;
                    int lineNumber = 1;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lineNumber++;
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        try
                        {
                            var values = ParseCsvLine(line);

                            if (values.Count < 11) continue; // Need at least 11 columns

                            if (!int.TryParse(values[0], out int empNo)) continue;

                            var staff = new StaffHRImportModel
                            {
                                EmployeeNumber = empNo,
                                FullName = values[1]?.Trim() ?? "",
                                HierarchyLevel1 = values[2]?.Trim(),
                                HierarchyLevel2 = values[3]?.Trim(),
                                HierarchyLevel3 = values[4]?.Trim(),
                                HierarchyLevel4 = values[5]?.Trim(),
                                HierarchyLevel5 = values[6]?.Trim(),
                                JobTitle = values[7]?.Trim() ?? "",
                                ContractType = values[8]?.Trim() ?? "",
                                ManagerEmployeeNumber = null,
                                ManagerName = values[10]?.Trim()
                            };

                            // Parse manager employee number if present
                            if (!string.IsNullOrWhiteSpace(values[9]) && int.TryParse(values[9], out int manEmpNo))
                            {
                                staff.ManagerEmployeeNumber = manEmpNo;
                            }

                            staffList.Add(staff);
                        }
                        catch (Exception)
                        {
                            // Skip rows that have issues
                            continue;
                        }
                    }
                }
                // Handle JSON files
                else if (fileExtension == ".json")
                {
                    using var reader = new StreamReader(file.OpenReadStream());
                    var json = await reader.ReadToEndAsync();
                    staffList = System.Text.Json.JsonSerializer.Deserialize<List<StaffHRImportModel>>(json) ?? new List<StaffHRImportModel>();
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid file format. Please upload a CSV or JSON file.";
                    return RedirectToAction(nameof(Index));
                }

                if (!staffList.Any())
                {
                    TempData["ErrorMessage"] = "No valid staff data found in the file.";
                    return RedirectToAction(nameof(Index));
                }

                // Group by EmployeeNumber and merge duplicates within the CSV
                var groupedStaff = staffList
                    .GroupBy(s => s.EmployeeNumber)
                    .Select(g => MergeDuplicateStaff(g.ToList()))
                    .ToList();

                int added = 0;
                int updated = 0;
                int merged = staffList.Count - groupedStaff.Count;

                foreach (var item in groupedStaff)
                {
                    var existing = await _context.StaffHR
                        .FirstOrDefaultAsync(s => s.EmployeeNumber == item.EmployeeNumber);

                    if (existing != null)
                    {
                        // Update existing record
                        existing.FullName = item.FullName;
                        existing.HierarchyLevel1 = item.HierarchyLevel1;
                        existing.HierarchyLevel2 = item.HierarchyLevel2;
                        existing.HierarchyLevel3 = item.HierarchyLevel3;
                        existing.HierarchyLevel4 = item.HierarchyLevel4;
                        existing.HierarchyLevel5 = item.HierarchyLevel5;
                        existing.JobTitle = item.JobTitle;
                        existing.ContractType = item.ContractType;
                        existing.ManagerEmployeeNumber = item.ManagerEmployeeNumber;
                        existing.ManagerName = item.ManagerName;
                        existing.LastUpdated = DateTime.UtcNow;
                        updated++;
                    }
                    else
                    {
                        // Add new record
                        var newStaff = new StaffHR
                        {
                            EmployeeNumber = item.EmployeeNumber,
                            FullName = item.FullName,
                            HierarchyLevel1 = item.HierarchyLevel1,
                            HierarchyLevel2 = item.HierarchyLevel2,
                            HierarchyLevel3 = item.HierarchyLevel3,
                            HierarchyLevel4 = item.HierarchyLevel4,
                            HierarchyLevel5 = item.HierarchyLevel5,
                            JobTitle = item.JobTitle,
                            ContractType = item.ContractType,
                            ManagerEmployeeNumber = item.ManagerEmployeeNumber,
                            ManagerName = item.ManagerName,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        };
                        _context.StaffHR.Add(newStaff);
                        added++;
                    }
                }

                await _context.SaveChangesAsync();

                var message = $"Import completed: {added} added, {updated} updated";
                if (merged > 0)
                {
                    message += $", {merged} duplicates merged";
                }
                TempData["SuccessMessage"] = message + ".";
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                TempData["ErrorMessage"] = $"Error importing data: {innerMessage}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StaffHRExists(int id)
        {
            return _context.StaffHR.Any(e => e.Id == id);
        }

        // Helper method to merge duplicate staff entries (same employee number, different roles)
        private StaffHRImportModel MergeDuplicateStaff(List<StaffHRImportModel> duplicates)
        {
            if (duplicates.Count == 1)
                return duplicates[0];

            var merged = duplicates[0];

            // Merge job titles with " / " separator
            var jobTitles = duplicates
                .Select(d => d.JobTitle)
                .Where(j => !string.IsNullOrWhiteSpace(j))
                .Distinct()
                .ToList();
            merged.JobTitle = string.Join(" / ", jobTitles);

            // Merge contract types
            var contractTypes = duplicates
                .Select(d => d.ContractType)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();
            merged.ContractType = string.Join(" / ", contractTypes);

            // For hierarchy, use the first non-empty value, or merge if different
            merged.HierarchyLevel1 = GetMergedValue(duplicates.Select(d => d.HierarchyLevel1));
            merged.HierarchyLevel2 = GetMergedValue(duplicates.Select(d => d.HierarchyLevel2));
            merged.HierarchyLevel3 = GetMergedValue(duplicates.Select(d => d.HierarchyLevel3));
            merged.HierarchyLevel4 = GetMergedValue(duplicates.Select(d => d.HierarchyLevel4));
            merged.HierarchyLevel5 = GetMergedValue(duplicates.Select(d => d.HierarchyLevel5));

            return merged;
        }

        private string? GetMergedValue(IEnumerable<string?> values)
        {
            var distinctValues = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct()
                .ToList();

            if (distinctValues.Count == 0)
                return null;

            if (distinctValues.Count == 1)
                return distinctValues[0];

            // If multiple different values, merge with " / "
            return string.Join(" / ", distinctValues);
        }

        // Helper method to parse CSV line, handling quoted fields with commas
        private List<string> ParseCsvLine(string line)
        {
            var values = new List<string>();
            var currentValue = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            // Add the last value
            values.Add(currentValue.ToString());

            return values;
        }

        // GET: HR/PDRTracking
        [HRAuthorize]
        public async Task<IActionResult> PDRTracking(int? year, string? period)
        {
            // Default to current year and period if not specified
            if (!year.HasValue)
                year = DateTime.Now.Year;

            if (string.IsNullOrEmpty(period))
                period = PDRPeriod.GetCurrentPeriod();

            // Get all active staff from HR system
            var allStaff = await _context.StaffHR
                .Where(s => s.IsActive)
                .OrderBy(s => s.FullName)
                .ToListAsync();

            // Get all staff references
            var staffReferences = allStaff
                .Select(s => s.EmployeeNumber.ToString().PadLeft(8, '0'))
                .ToList();

            // Get all PDRs for the selected year and period
            var pdrs = await _context.PDRs
                .Where(p => p.Year == year.Value && p.Period == period)
                .ToListAsync();

            // Load AD staff data for all staff
            var employeeNumbers = allStaff.Select(s => s.EmployeeNumber).ToList();
            var staffAD = await _context.Staff
                .Where(s => employeeNumbers.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s);

            // Create tracking records for each staff member
            var trackingRecords = new List<PDRTrackingRecord>();
            foreach (var staff in allStaff)
            {
                var pdr = pdrs.FirstOrDefault(p => p.StaffReference == staff.EmployeeNumber.ToString().PadLeft(8, '0'));
                var ad = staffAD.ContainsKey(staff.EmployeeNumber) ? staffAD[staff.EmployeeNumber] : null;

                trackingRecords.Add(new PDRTrackingRecord
                {
                    StaffHR = staff,
                    StaffAD = ad,
                    PDR = pdr,
                    HasPDR = pdr != null,
                    Status = pdr?.Status ?? PDRStatus.Assigned,
                    IsOverdue = pdr?.DueDate.HasValue == true && pdr.DueDate.Value < DateTime.Now && pdr.Status != PDRStatus.Completed
                });
            }

            // Calculate summary statistics
            var totalStaff = trackingRecords.Count;
            var noPDRAssigned = trackingRecords.Count(r => !r.HasPDR);
            var assigned = trackingRecords.Count(r => r.HasPDR && r.Status == PDRStatus.Assigned);
            var staffCompleted = trackingRecords.Count(r => r.HasPDR && r.Status == PDRStatus.StaffCompleted);
            var managerCompleted = trackingRecords.Count(r => r.HasPDR && r.Status == PDRStatus.ManagerCompleted);
            var readyForCollaboration = trackingRecords.Count(r => r.HasPDR && r.Status == PDRStatus.ReadyForCollaboration);
            var completed = trackingRecords.Count(r => r.HasPDR && r.Status == PDRStatus.Completed);
            var overdue = trackingRecords.Count(r => r.IsOverdue);

            var viewModel = new PDRTrackingViewModel
            {
                Year = year.Value,
                Period = period,
                PeriodDisplayName = PDRPeriod.GetDisplayName(period),
                TrackingRecords = trackingRecords,
                TotalStaff = totalStaff,
                NoPDRAssigned = noPDRAssigned,
                Assigned = assigned,
                StaffCompleted = staffCompleted,
                ManagerCompleted = managerCompleted,
                ReadyForCollaboration = readyForCollaboration,
                Completed = completed,
                Overdue = overdue,
                CompletionRate = totalStaff > 0 ? (completed * 100.0 / totalStaff) : 0,
                AvailableYears = new List<int> { year.Value - 1, year.Value, year.Value + 1 },
                AvailablePeriods = PDRPeriod.AllPeriods
            };

            return View(viewModel);
        }

        // Helper model for bulk import - matches the HR Excel sheet JSON format
        public class StaffHRImportModel
        {
            [System.Text.Json.Serialization.JsonPropertyName("Emp No.")]
            public int EmployeeNumber { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("Full Name")]
            public string FullName { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("Hier Lvl1")]
            public string? HierarchyLevel1 { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("Hier Lvl2")]
            public string? HierarchyLevel2 { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("Hier Lvl3")]
            public string? HierarchyLevel3 { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("Hier Lvl4")]
            public string? HierarchyLevel4 { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("Hier Lvl5")]
            public string? HierarchyLevel5 { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("Post LDesc")]
            public string JobTitle { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("Contract type")]
            public string ContractType { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("Man Employee Number")]
            public int? ManagerEmployeeNumber { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("Man Name")]
            public string? ManagerName { get; set; }
        }
    }

    // Helper classes for department analysis
    public class DepartmentInfo
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Count { get; set; }
        public List<int> EmployeeNumbers { get; set; } = new List<int>();
    }

    public class SimilarDepartmentGroup
    {
        public List<DepartmentInfo> Departments { get; set; } = new List<DepartmentInfo>();
        public int TotalCount => Departments.Sum(d => d.Count);
        public int TotalEmployees => Departments.SelectMany(d => d.EmployeeNumbers).Distinct().Count();
    }

    // PDR Tracking View Models
    public class PDRTrackingViewModel
    {
        public int Year { get; set; }
        public string Period { get; set; } = string.Empty;
        public string PeriodDisplayName { get; set; } = string.Empty;
        public List<PDRTrackingRecord> TrackingRecords { get; set; } = new List<PDRTrackingRecord>();
        public int TotalStaff { get; set; }
        public int NoPDRAssigned { get; set; }
        public int Assigned { get; set; }
        public int StaffCompleted { get; set; }
        public int ManagerCompleted { get; set; }
        public int ReadyForCollaboration { get; set; }
        public int Completed { get; set; }
        public int Overdue { get; set; }
        public double CompletionRate { get; set; }
        public List<int> AvailableYears { get; set; } = new List<int>();
        public List<string> AvailablePeriods { get; set; } = new List<string>();
    }

    public class PDRTrackingRecord
    {
        public StaffHR StaffHR { get; set; } = null!;
        public Staff? StaffAD { get; set; }
        public PDR? PDR { get; set; }
        public bool HasPDR { get; set; }
        public PDRStatus Status { get; set; }
        public bool IsOverdue { get; set; }
    }
}
