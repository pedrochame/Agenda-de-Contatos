using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Agenda_de_Contatos
{
    //Classe Contato, com atributos de nome, telefone e email
    class Contato {

        public string Nome, Telefone, Email;
        public Contato(string n, string t, string e) {
            Nome = n;
            Telefone = t;
            Email = e;
        }
        
    }

    internal class Program
    {

        //Função para validar telefone
        static bool validaTel(string tel)
        {
            //Formato Válido: (99) 99999-9999

            if (tel.Length != 15)
            {
                return false;
            }

            for (int i = 0; i < tel.Length; i++)
            {
                char c = tel[i];

                if (i == 0)
                {
                    if (c != '(')
                    {
                        return false;
                    }
                }

                if (i == 3)
                {
                    if (c != ')')
                    {
                        return false;
                    }
                }

                if (i == 4)
                {
                    if (c != ' ')
                    {
                        return false;
                    }
                }

                if (i == 10)
                {
                    if (c != '-')
                    {
                        return false;
                    }
                }

                if (i == 5)
                {
                    if (c != '9')
                    {
                        return false;
                    }
                }

                if (i == 1 || i == 2 || i == 6 || i == 7 || i == 8 || i == 9 || i == 11 || i == 12 || i == 13 || i == 14)
                {
                    if (c != '0' || c != '1' || c != '2' || c != '3' || c != '4' || c != '5' || c != '6' || c != '7' || c != '8' || c != '9')
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        //Definindo a string de conexão para o banco de dados SQLite.
        static string connectionString = "Data Source=AgendaContatos.db;Version=3;";

        //Opções do Menu Principal
        enum opcaoMenu { Adicionar, Editar, Remover, Relatório, Sair };

        //Campos da tabela Contatos
        enum opcaoCampos { Nome, Telefone, Email };

        //Função que recebe um id e retorna o objeto do tipo Contato correspondente
        static Contato retornaContato(int id)
        {

            Contato c = new Contato("","","");

            string n = "", t = "", e="";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string selectFromTable = $"select nome,tel,email from contatos where id = {id}";
                    using (SQLiteCommand command = new SQLiteCommand(selectFromTable, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                         
                            while (reader.Read())
                            {
                                n = reader.GetString(0);
                                t = reader.GetString(1);
                                e = reader.GetString(2);
                            }
                        }
                    }

                    connection.Dispose();

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao conectar ao banco de dados: {ex.Message}");
                }
            }

            c.Nome = n;
            c.Telefone = t;
            c.Email = e;

            return c;
        }


        //Função que retorna uma lista contendo todos os id's dos registros da tabela contatos
        static List<int> listaIds() {

            List<int> x = new List<int>();
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string sql = "select * from contatos";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                x.Add(id);
                            }
                        }
                    }

                    connection.Dispose();

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao conectar ao banco de dados: {ex.Message}");
                }
            }

            return x;
        }
        
        //Função para adicionar registro na tabela contatos
        static void Adicionar() {

            while (true)
            {
                Console.Clear();
                Relatorio();

                Console.Write("\nDeseja adicionar um contato? (1 - Sim | 0 - Não): ");
                int resp = int.Parse(Console.ReadLine());

                if(resp == 0)
                {
                    break;
                }

                Console.Write("\nNome: ");
                string nome = Console.ReadLine();
                Console.Write("Telefone: ");
                string tel = Console.ReadLine();
                Console.Write("E-mail: ");
                string email = Console.ReadLine();

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        string insertIntoTable = $"insert into contatos values(null, '{nome}','{tel}','{email}')";
                        using (SQLiteCommand command = new SQLiteCommand(insertIntoTable, connection))
                        {
                            command.ExecuteNonQuery();
                        }

                        connection.Dispose();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao conectar ao banco de dados: {ex.Message}");
                    }
                }

                Console.Write("\nContato adicionado com sucesso. Tecle para continuar.");
                Console.ReadLine();
            }
        }

        //Função para editar registros da tabela contatos
        static void Editar()
        {

            while (true)
            {
                Console.Clear();
                Relatorio();

                if (listaIds().Count == 0)
                {
                    break;
                }

                Console.Write("\nDigite o identificador para edição (digite 0 para voltar): ");
                int id_editar = int.Parse(Console.ReadLine());

                if(id_editar == 0)
                {
                    break;
                }

                int achou = 0;
                foreach (int i in listaIds()) {
                    if (id_editar == i)
                    {
                        achou=1;
                    }
                }

                if (achou == 0)
                {
                    Console.Write("\nDigite um identificador válido. Tecle para continuar.");
                    Console.ReadLine();
                }
                else
                {

                    int erro = 0;
                    int sucesso = 1;

                    while (true)
                    {
                        erro = 0;
                        sucesso = 1;

                        Console.WriteLine("\n1 - Nome\n2 - Telefone\n3 - E-mail\n");

                        Console.Write("Digite o campo para edição (digite 0 para voltar): ");
                        int campo_editar = -1 + int.Parse(Console.ReadLine());

                        if (campo_editar == -1)
                        {
                            erro = 1;
                            break;
                        }

                        opcaoCampos op = (opcaoCampos)campo_editar;

                        switch (op)
                        {

                            case opcaoCampos.Nome:

                                string antigoNome = retornaContato(id_editar).Nome;
                                Console.Write($"\nEntre com o novo nome: {antigoNome} -> ");
                                string nome = Console.ReadLine();

                                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                                {
                                    try
                                    {
                                        connection.Open();

                                        string updateTable = $"update contatos set nome = '{nome}' where id = {id_editar}";
                                        using (SQLiteCommand command = new SQLiteCommand(updateTable, connection))
                                        {
                                            command.ExecuteNonQuery();
                                        }

                                        connection.Dispose();

                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Erro ao conectar ao banco de dados: {ex.Message}");
                                    }
                                }

                                break;

                            case opcaoCampos.Telefone:
                                string antigoTelefone = retornaContato(id_editar).Telefone;
                                Console.Write($"\nEntre com o novo telefone: {antigoTelefone} -> ");
                                string tel = Console.ReadLine();

                                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                                {
                                    try
                                    {
                                        connection.Open();

                                        string updateTable = $"update contatos set tel = '{tel}' where id = {id_editar}";
                                        using (SQLiteCommand command = new SQLiteCommand(updateTable, connection))
                                        {
                                            command.ExecuteNonQuery();
                                        }

                                        connection.Dispose();

                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Erro ao conectar ao banco de dados: {ex.Message}");
                                    }
                                }

                                break;

                            case opcaoCampos.Email:
                                string antigoEmail = retornaContato(id_editar).Email;
                                Console.Write($"\nEntre com o novo email: {antigoEmail} -> ");
                                string email = Console.ReadLine();

                                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                                {
                                    try
                                    {
                                        connection.Open();

                                        string updateTable = $"update contatos set email = '{email}' where id = {id_editar}";
                                        using (SQLiteCommand command = new SQLiteCommand(updateTable, connection))
                                        {
                                            command.ExecuteNonQuery();
                                        }

                                        connection.Dispose();

                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Erro ao conectar ao banco de dados: {ex.Message}");
                                    }
                                }

                                break;

                            default:
                                Console.Write("\nOpção inválida. Tecle para continuar.");
                                Console.ReadLine();
                                erro = 1;
                                sucesso = 0;
                                break;

                        }
                        if (sucesso == 1)
                        {
                            break;
                        }
                    }

                    if (erro == 0)
                    {
                        Console.Write("\nContato editado com sucesso. Tecle para continuar.");
                        Console.ReadLine();
                    }

                }    
            }
        }

        //Função para remover registros da tabela contatos
        static void Remover()
        {

            while (true)
            {
                Console.Clear();

                Relatorio();

                if (listaIds().Count == 0)
                {
                    break;
                }

                Console.Write("\nDigite o identificador para exclusão (digite 0 para voltar): ");
                int id_para_excluir = int.Parse(Console.ReadLine());

                if (id_para_excluir == 0)
                {
                    break;
                }

                int achou = 0;
                foreach (int i in listaIds())
                {
                    if(i == id_para_excluir)
                    {
                        achou = 1;
                        break;
                    }
                }

                if (achou == 0)
                {
                    Console.Write("\nDigite um identificador válido. Tecle para continuar.");
                    Console.ReadLine();
                }
                else
                {


                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                    {
                        try
                        {
                            connection.Open();

                            string insertIntoTable = $"delete from contatos where id = {id_para_excluir}";
                            using (SQLiteCommand command = new SQLiteCommand(insertIntoTable, connection))
                            {
                                command.ExecuteNonQuery();
                            }

                            connection.Dispose();

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao conectar ao banco de dados: {ex.Message}");
                        }
                    }


                    Console.Write("\nContato removido com sucesso. Tecle para continuar.");
                    Console.ReadLine();
                }
            }
        }

        //Função que imprime todos os registros e seus campos, da tabela contatos
        static void Relatorio()
        {

            if (listaIds().Count > 0)
            {

                Console.WriteLine("Lista de Contatos:\n");

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        string selectFromTable = "select * from contatos";
                        using (SQLiteCommand command = new SQLiteCommand(selectFromTable, connection))
                        {
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                // Loop para ler e imprimir os resultados.
                                while (reader.Read())
                                {
                                    int id = reader.GetInt32(0); // O índice 0 corresponde à coluna id.
                                    string nome = reader.GetString(1); // O índice 1 corresponde à coluna nome
                                    string tel = reader.GetString(2); // O índice 2 corresponde à coluna tel.
                                    string email = reader.GetString(3); // O índice 3 corresponde à coluna email.

                                    Console.WriteLine($"Id: {id}, Nome: {nome}, Telefone: {tel}, E-mail: {email}\n");
                                }
                            }
                        }

                        connection.Dispose();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao conectar ao banco de dados: {ex.Message}");
                    }
                }
                Console.Write("Tecle para continuar.");
                Console.ReadLine();
            }
            else
            {
                Console.Write("Não há contatos. Tecle para continuar.");
                Console.ReadLine();
            }
        }

        //Programa Principal
        static void Main(string[] args)
        {
            //Conexão com o Banco de Dados

            //Criando uma conexão com o banco de dados SQLite.
            using (SQLiteConnection connection = new SQLiteConnection(connectionString)){
                try{

                    connection.Open();

                    //Criando a tabela de contatos
                    string createTableSql = "CREATE TABLE IF NOT EXISTS contatos (id integer primary key autoincrement, nome text, tel text, email text)";
                    using (SQLiteCommand command = new SQLiteCommand(createTableSql, connection)){
                        command.ExecuteNonQuery();
                    }

                    connection.Dispose();

                }
                catch (Exception ex){
                    Console.WriteLine($"Erro ao conectar ao banco de dados: {ex.Message}");
                }
            }

            //Menu Principal
            while (true){
                Console.Clear();
                Console.WriteLine("Agenda De Contatos:\n");
                Console.WriteLine("1 - Adicionar\n2 - Editar\n3 - Remover\n4 - Relatório\n5 - Sair\n");

                int entrada = -1 + int.Parse(Console.ReadLine());

                opcaoMenu op = (opcaoMenu) entrada;

                switch (op)
                {
                    case opcaoMenu.Adicionar:
                        Console.Write("\nVocê escolheu adicionar um novo contato. Tecle para continuar.");
                        Console.ReadLine();
                        Console.Clear();
                        Adicionar();
                        break;
                    case opcaoMenu.Editar:
                        Console.Write("\nVocê escolheu editar um contato. Tecle para continuar.");
                        Console.ReadLine();
                        Console.Clear();
                        Editar();
                        break;
                    case opcaoMenu.Remover:
                        Console.Write("\nVocê escolheu remover um contato. Tecle para continuar.");
                        Console.ReadLine();
                        Console.Clear();
                        Remover();
                        break;
                    case opcaoMenu.Relatório:
                        Console.Write("\nVocê escolheu ver o relatório de contatos. Tecle para continuar.");
                        Console.ReadLine();
                        Console.Clear();
                        Relatorio();
                        break;
                    case opcaoMenu.Sair:
                        Console.Write("\nVocê escolheu sair. Tecle para continuar.");
                        Console.ReadLine(); 
                        Environment.Exit(0);
                        break;
                    default:
                        Console.Write("\nEscolha uma opção entre 1 e 5. Tecle para continuar.");
                        Console.ReadLine();
                        break;
                }
            }
        }
    }
}