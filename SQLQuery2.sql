create database Book_Store
create table Users (
	id int IDENTITY(1,1) PRIMARY KEY,
	account varchar(50),
	pass_word varchar(50),
	mail varchar (50),
	lever int,
	avatar varchar(255)
)