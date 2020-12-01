create database Book_Store
create table Users (
	id int IDENTITY(1,1) PRIMARY KEY,
	account varchar(50),
	pass_word varchar(50),
	mail varchar (50),
	lever int,
	avatar varchar(255)
)



/*them 28/11/2020*/

alter table Users 
add shippingAddress varchar(500),
payment varchar(26),
promocodes varchar(20),
phone varchar(20)