# AI-Powered Invoice Generator: .NET vs. Java Implementation

## .NET Implementation

The .NET version of this project is built using **ASP.NET Core MVC**, leveraging Microsoft's identity provider for authentication and user management. It integrates seamlessly with **Entity Framework Core** for database management and supports **dependency injection** to structure the AI-driven processing of Git commit messages. The application follows a **model-view-controller (MVC)** architecture, making it easy to extend and customize. The OpenAI API is integrated through standard **HTTP client services**, and the system processes CSV files using built-in **.NET file I/O and LINQ** for structured data handling.

The .NET implementation is well-suited for teams already working within the Microsoft ecosystem, providing seamless integration with Azure, SQL Server, and other Microsoft services. The use of **RAG (Retrieval-Augmented Generation)** is structured within the ASP.NET Core middleware, allowing developers to extend AI capabilities without modifying core logic.

---

## Java Implementation

The Java implementation follows a **Spring Boot** architecture, using **Spring Security** for authentication and role-based access control. Instead of Entity Framework, it utilizes **Hibernate ORM** for database operations, offering flexibility for teams using MySQL, PostgreSQL, or other relational databases. The OpenAI API integration is managed using **Spring WebClient or RestTemplate**, providing reactive and synchronous request handling.

For CSV processing, Java uses **Apache Commons CSV or OpenCSV**, ensuring efficient parsing and data manipulation. The RAG-based enhancements are integrated within **Spring Beans**, allowing for modular AI-driven responses. This version is ideal for teams working in an enterprise Java environment, with compatibility for cloud platforms like AWS, Google Cloud, and on-premise Java application servers.

---

## Key Differences

- **Frameworks & Architecture**: ASP.NET Core MVC vs. Spring Boot  
- **Authentication**: Microsoft Identity Provider vs. Spring Security  
- **Database Handling**: Entity Framework Core vs. Hibernate ORM  
- **CSV Processing**: LINQ-based parsing vs. Apache Commons CSV/OpenCSV  
- **API Integration**: .NET HttpClient vs. Spring WebClient/RestTemplate  
- **Cloud Ecosystem**: Azure-first approach vs. AWS/GCP compatibility  

Both implementations provide a structured approach to integrating AI into software development workflows, ensuring that Git commit messages are converted into meaningful, invoice-ready time entries.
