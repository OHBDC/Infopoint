# InfoPoint - Staff Management System

InfoPoint is an ASP.NET Core MVC web application designed for managing Learning Walks and Performance Development Reviews (PDRs) for educational institutions. The application uses Google OAuth for authentication, restricting access to staff with @g.bdc.ac.uk email addresses.

## Prerequisites

- .NET 8.0 SDK or later
- SQLite (included with .NET, cross-platform)
- Visual Studio 2022 / Visual Studio Code / Rider
- Google Cloud Console account for OAuth setup

## Initial Setup

### 1. Clone the Repository

```bash
git clone [repository-url]
cd InfoPoint
```

### 2. Configure Google OAuth

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google+ API
4. Go to "Credentials" and create OAuth 2.0 Client ID
5. Set the following authorized redirect URIs:
   - `https://localhost:5051/signin-google`
   - `http://localhost:5050/signin-google` (for development)
   - `https://localhost:5001/signin-google` (fallback)
   - `http://localhost:5000/signin-google` (fallback)
6. Copy your Client ID and Client Secret

### 3. Update Application Settings

Update `appsettings.json` with your Google OAuth credentials:

```json
{
  "GoogleAuthentication": {
    "ClientId": "YOUR_ACTUAL_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_ACTUAL_GOOGLE_CLIENT_SECRET"
  }
}
```

**Important**: Never commit real credentials to source control. Consider using User Secrets for development:

```bash
dotnet user-secrets init
dotnet user-secrets set "GoogleAuthentication:ClientId" "your-client-id"
dotnet user-secrets set "GoogleAuthentication:ClientSecret" "your-client-secret"
```

### 4. Update Database Connection String

The application uses SQLite by default, which requires no additional setup. The connection string is already configured:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=InfoPoint.db"
  }
}
```

For production, you may want to use SQL Server or another database provider.

### 5. Create Database

Run the following commands to create the database:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

If Entity Framework Core tools are not installed globally:

```bash
dotnet tool install --global dotnet-ef
```

### 6. Run the Application

```bash
dotnet run --urls="http://localhost:5050;https://localhost:5051"
```

Navigate to `https://localhost:5051` or `http://localhost:5050`

## Project Structure

```
InfoPoint/
├── Areas/
│   ├── LearningWalks/          # Learning Walks module
│   │   ├── Controllers/
│   │   ├── Models/
│   │   └── Views/
│   └── PDRs/                   # PDRs module
│       ├── Controllers/
│       ├── Models/
│       └── Views/
├── Controllers/
│   ├── AccountController.cs    # Google OAuth authentication
│   └── HomeController.cs       # Dashboard and main pages
├── Data/
│   └── ApplicationDbContext.cs # Entity Framework context
├── Models/
│   └── ApplicationUser.cs      # Extended Identity user
├── Views/
│   ├── Account/               # Login and authentication views
│   ├── Home/                  # Dashboard and home views
│   └── Shared/                # Layout and shared components
└── Program.cs                 # Application configuration
```

## Features

### Authentication
- Google OAuth integration
- Restricted to @g.bdc.ac.uk email addresses only
- Automatic user account creation on first login
- User profile information from Google

### Areas
1. **Learning Walks**
   - Manage classroom observations
   - Schedule learning walks
   - Record findings
   - Generate reports

2. **PDRs (Performance Development Reviews)**
   - Schedule PDR meetings
   - Set and track objectives
   - Performance tracking
   - Generate PDR reports

### Security Features
- Domain validation (@g.bdc.ac.uk only)
- ASP.NET Core Identity integration
- Authorize attributes protecting all areas
- Secure logout functionality

## Development Guidelines

### For Two Developers

1. **Developer 1**: Focus on Learning Walks area
   - Work in `Areas/LearningWalks/`
   - Create models, views, and controllers for Learning Walks functionality

2. **Developer 2**: Focus on PDRs area
   - Work in `Areas/PDRs/`
   - Create models, views, and controllers for PDRs functionality

### Git Workflow

1. Create feature branches for each area:
   ```bash
   git checkout -b feature/learning-walks
   git checkout -b feature/pdrs
   ```

2. Work independently in your assigned area
3. Regular commits and pull requests
4. Merge to main branch after code review

### Adding New Features

1. Create models in your area's Models folder
2. Add controllers with proper authorization
3. Create views following the existing layout
4. Update navigation in `_Layout.cshtml` if needed

## Troubleshooting

### Google OAuth Issues

1. **"Access Denied" error**: Ensure you're using a @g.bdc.ac.uk email
2. **Redirect URI mismatch**: Check that your Google Console redirect URIs match exactly
3. **Invalid credentials**: Verify Client ID and Secret are correct

### Database Issues

1. **Cannot connect to SQL Server**: Check connection string
2. **Migration errors**: Delete Migrations folder and recreate
3. **LocalDB not installed**: Install SQL Server Express LocalDB

### Development Tips

- Use `dotnet watch run` for hot reload during development
- Check browser console for JavaScript errors
- Enable detailed error pages in development mode
- Use browser dev tools to inspect authentication cookies

## Production Deployment

1. Update `appsettings.Production.json` with production settings
2. Use environment variables or Azure Key Vault for secrets
3. Update Google OAuth redirect URIs for production domain
4. Enable HTTPS and configure SSL certificates
5. Set up proper logging and monitoring

## Support

For issues or questions:
1. Check the troubleshooting section
2. Review application logs
3. Contact the development team

## License

[Your License Here]

---

Built with ASP.NET Core 8.0 and Entity Framework Core