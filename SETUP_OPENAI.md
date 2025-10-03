# Setting Up OpenAI Integration

## Important: API Key Security

**NEVER commit your API key to source control or share it publicly.**

## Setup Instructions

1. **Get your OpenAI API Key**
   - Go to https://platform.openai.com/api-keys
   - Create a new API key
   - Copy the key (you won't be able to see it again)

2. **Configure the API Key in your application**

   For development, use one of these methods:

   ### Option 1: User Secrets (Recommended for Development)
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "OpenAI:ApiKey" "your-api-key-here"
   ```

   ### Option 2: Environment Variable
   Set an environment variable:
   ```bash
   export OpenAI__ApiKey="your-api-key-here"
   ```

   ### Option 3: appsettings.Development.json (DO NOT commit this file)
   Create or update `appsettings.Development.json`:
   ```json
   {
     "OpenAI": {
       "ApiKey": "your-api-key-here"
     }
   }
   ```

3. **For Production**
   - Use Azure Key Vault, AWS Secrets Manager, or similar
   - Set as an environment variable in your hosting environment
   - Never store in appsettings.json or code

## Testing the Integration

1. Run the application
2. Navigate to a PDR collaboration form
3. Add a SMART target
4. Enter some rough target text in the details field
5. Click "Enhance with AI" button
6. The text should be enhanced to a proper SMART target

## Troubleshooting

- If you see "Unable to enhance target", check your API key is correctly configured
- Check the console logs for any error messages
- Ensure you have credits in your OpenAI account