\# 📦 Project Overview



\## 🚀 Initial Steps

\- Identify main bugs  

\- Define the scope of the software and missing logical functionality  

\- Identify main entities of the software  

\- Identify key functionalities required for a new solution  

\- Design and plan a modern replacement  



\---



\## 🐞 Main Bugs

\- Syntax error in `Order Repository`  





\---



\## ⚠️ Current System Issues

\- No validation for user input  

\- User must already know the product name  

\- Order total is not saved  

\- Cannot list available products  

\- Cannot create an order with multiple products  

\- No way to read/view orders  

\- No way to add new products  

\- Customer is stored only as a text field  



> ⚡ Note: Some issues can be fixed, but since the system will be replaced, focus is on the new implementation.



\---



\## 🎯 Scope and Missing Functionality



The system is used to store and manage orders.



\### Main Domains

\- Product  

\- Products  

\- Customers  



> The current system is a console application.  

> It will be replaced with a \*\*web application\*\* for better accessibility.



\---



\## 🏗️ Proposed Architecture



A scalable solution is to adopt a \*\*microservices architecture\*\*.



\### Core Services

\- Product Service  

\- Contact Service \*(renamed from Customer for flexibility)\*  

\- Order Service  



\### Additional Services

\- Auth Service \*(restricted access)\*  

\- Log Service \*(tracks errors and operations)\*  



\---



\## 🔧 Shared Components

\- API Gateway  

\- Redis AppService \*(shared caching)\*  

\- RabbitMQ \*(messaging via shared library)\*  

\- Middleware:

\- `X-Correlation-ID` tracking  

\- Automatic logging for failed requests  



\---



\## 🔄 Data Synchronization



\### Option 1 (Implemented)

\- Services query others when needed  

\- Results are cached  

\- Cache is cleared via events on updates  



\### Option 2 (Not Implemented)

\- Store external data locally  

\- Update via events when source changes  



\---



\## 🔌 Services \& Communication



\### 🔐 Auth Service

\- Login, Register  



\*\*Future\*\*

\- User blocking  

\- Roles \& permissions  



\*\*Communication\*\*

\- Current: Username stored in JWT \*(not ideal)\*  

\- Future:

\- Subject ID in JWT  

\- Shared Account Profile via messaging  



\---



\### 📦 Product Service

\- Create, Edit, Delete, Get  



\*\*Communication\*\*

\- Emits events on update/delete  



\---



\### 👤 Contact Service

\- Create, Edit, Delete, Get  



\*\*Communication\*\*

\- Emits events on update/delete  



\---



\### 🧾 Order Service

\- Create, Edit, Delete, Get  

\- Add item / Remove item  

\- Status \*(enum → should become table)\*  



\*\*Communication\*\*

\- Fetches Product \& Contact data and caches it  

\- Clears cache on update events  

\- Edit functionality exists but currently disabled  



\---



\### 📝 Log Service

\- Logs sent via RabbitMQ \*(async)\*  

\- Currently stored in text files \*(not ideal)\*  



\*\*Future\*\*

\- ElasticSearch or database storage  



\---



\## 💻 Frontend



> Built quickly — functional but incomplete.



\### Features

\- Login page  

\- Route protection based on authentication  

\- Contact management (CRUD)  

\- Product management (CRUD)  

\- Order section:

\- List page  

\- Create/Edit page  



\---



\## 🔮 Future Improvements



\### ⚙️ Backend

\- Improve error handling  

\- Implement roles \& permissions  

\- Add full user management APIs  

\- Sync users across services via RabbitMQ  

\- Improve data models (especially Orders)  

\- Add pagination, sorting, filtering  

\- Reporting \& export features  

\- Improve logging (ElasticSearch / DB)  

\- Add unit tests  

\- Configure environments  

\- Containerization (Docker / Kubernetes)  



\---



\### 🎨 Frontend

\- Form validation  

\- Loading spinners \& confirmation dialogs  

\- Sorting \& filtering  

\- Role-based access control  

\- User management section  

\- CSV / Excel import \& export  



\---





