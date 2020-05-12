﻿using Phonebook.Entities;
using Phonebook.Repositories;
using System;

namespace Phonebook.Views.UserViews
{
    public class UpdateUserView
    {
        public void Show()
        {
            Console.Clear();
            Console.Write("Input user's id to update:");
            bool isUserIdNumber = uint.TryParse(Console.ReadLine(), out uint userInputId);

            if (!isUserIdNumber)
            {
                Console.WriteLine("Please input positive number.");
                Console.ReadKey();
                return;
            }

            //TODO: get user from db 
            // if user is null
            var userFromInput = new User(userInputId);

            UserRepository userRepository = new UserRepository();
            var userFromRepository = userRepository.ReadUser(userFromInput);

            if (userFromRepository == null) {
                Console.WriteLine("Invalid user id. User not found.");
                Console.ReadKey(true);
                return;
            }

            Console.Write("Username: ");
            string username = Console.ReadLine();

            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Invalid username.");
                return;
            }

            Console.Write("Password: ");
            string password = Console.ReadLine();

            if (string.IsNullOrEmpty(password))
            {
                Console.Write("Invalid password.");
                return;
            }

            Console.Write("First Name: ");
            string firstName = Console.ReadLine();

            if (string.IsNullOrEmpty(firstName))
            {
                Console.WriteLine("Invalid first name.");
                return;
            }

            Console.Write("Last Name: ");
            string lastName = Console.ReadLine();

            if (string.IsNullOrEmpty(lastName))
            {
                Console.WriteLine("Invalid last name.");
                return;
            }

            Console.Write("Admin (true/false): ");
            bool isAdminValueBool = bool.TryParse(Console.ReadLine(), out bool isAdmin);

            if (!isAdminValueBool)
            {
                Console.WriteLine("Invalid admin value.");
                return;
            }

            //update the user in db
            userFromInput = new User(userInputId, username, password, firstName, lastName, isAdmin);
            userRepository.UpdateUser(userFromInput);
        }
    }
}