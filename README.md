# Security PS's Intentionally Vulnerable Authentication Application
Security PS created an intentionally vulnerable ASP.NET MVC application with the goal of mimicking common flaws found in applications we assess. Both new and experienced security professionals can use this application to test their skill in bypassing steps of the authentication process and compromising other users' accounts. If you would like to start a career in software security, you live in the Kansas City area, and you're interested in working at Security PS then reach out by visiting: https://www.securityps.com and clicking "Contact Us". 

# Running the Application
## Using Docker
This application is bundled as a docker image. To download and run the image, use the following command:
```
docker run -d -p 5000:5000 ncoblentzsps/authenticationchallenge
```
The application will be available at http://localhost:5000/

## Without Docker
The application was developed using ASP.NET Core 2. If you choose to run it outside of Docker, then go to https://dotnet.microsoft.com and get the latest version of .NET Core for your platform. The application is cross platform so it will work on Windows, Linux, MAC OS X, and any other platform supported by Microsoft. The code builds successfully under both Visual Studio 2017 and 2019 and can be run both under IIS and standalone. If you are executing the code in an environment without Visual Studio, the following commands can be used:
```
dotnet restore
dotnet run
```
When using this method, the application will be available at http://localhost:5000/.

# Testing
Several accounts are created already to test with. Their details are below:

|Username|Password|SSN|Account Number|Account Balance|
|--------------|----------|---------|---------|----|
|test1@test.com|Passw0rd1!|123121231|111111111|1.11|
|test2@test.com|Passw0rd2!|123121232|222222222|1.12|
|test3@test.com|Passw0rd3!|123121233|333333333|1.13|
|test4@test.com|Passw0rd4!|123121234|444444444|1.14|

Also, there are three accounts you can attempt to compromise. They have been created with a random password, SSN, account number, and account balance. The username is of the format target###@test.com where the ### is a random three digit number (i.e. target853@test.com).

At first, practice finding vulnerabilities with the test1@test.com through test4@test.com. Try to skip process steps and compromise those accounts. Then move on to the target accounts. You can try things like
* Get a list of usernames in the application
* Skip steps of the authentication process for other users
* Gain full access to another user account
* and more...

Some of the exploits require you to use a proxy tool such as Burp Suite, OWASP ZAP, or Fiddler to exploit. Depending on your platform, you may have to add a host entry to be able to proxy traffic properly. On Windows add an entry to c:\windows\system32\drivers\etc\hosts such as: 
```
127.0.0.1 application.local
```
And then point your browser at http://application.local:5000
