import psycopg2
import bcrypt

class AccessService:
    def __init__(self, connection_string):
        self.connection_string = connection_string

    def _verify_password(self, raw_password: str, hashed_password: str) -> bool:
        return bcrypt.checkpw(raw_password.encode('utf-8'), hashed_password.encode('utf-8'))

    def authenticate_user(self, username: str, password: str) -> str | None:
        """Returns user id if authentication is correct, otherwise returns None"""
        connection = None
        cursor = None
        try:
            # Establish the connection
            connection = psycopg2.connect(self.connection_string)
            cursor = connection.cursor()

            query = """
                SELECT "Id", "Username", "Password"
                FROM "Users"
                WHERE "Username" = %s AND "IsDeleted" = false;
            """

            # Execute the query with parameters
            cursor.execute(query, (username,))
            user = cursor.fetchone()

            # If user not found, return False
            if not user:
                return None

            # Get the stored password hash
            stored_password_hash = user[2]

            # Compare the provided password with the stored hash using passlib
            if self._verify_password(password, stored_password_hash):
                return user[0]
            else:
                return None

        except Exception as e:
            raise e
        finally:
            # Ensure the cursor and connection are closed properly
            if cursor:
                cursor.close()
            if connection:
                connection.close()

    def check_pull_permission(self, repository_name: str, user_id: str) -> bool:
        connection = None
        cursor = None
        try:
            # Establish the connection
            connection = psycopg2.connect(self.connection_string)
            cursor = connection.cursor()

            query = """
                SELECT 1
                FROM "DockerRepositories" AS r                
                WHERE r."IsDeleted" = false AND r."Name" = %s
                AND (r."IsPublic" = TRUE
                    OR r."UserOwnerId" = %s
                    OR EXISTS (
                        SELECT 1
                        FROM "OrganizationMembers" AS om
                        WHERE om."OrganizationId" = r."Id" 
                            AND om."MemberId" = %s
                    ))
                LIMIT 1;
            """

            # Execute the query with parameters
            cursor.execute(query, (repository_name, user_id, user_id))

            # Fetch the result
            result = cursor.fetchone()

            return result is not None

        except Exception as e:
            raise e
        finally:
            if cursor:
                cursor.close()
            if connection:
                connection.close()

    def check_push_permission(self, repository_name: str, user_id: str) -> bool:
        connection = None
        cursor = None
        try:
            # Establish the connection
            connection = psycopg2.connect(self.connection_string)
            cursor = connection.cursor()

            query = """
                SELECT 1
                FROM "DockerRepositories" AS r
                WHERE r."IsDeleted" = false AND r."Name" = %s
                AND (
                    r."UserOwnerId" = %s
                    OR EXISTS (
                        SELECT 1
                        FROM "TeamPermissions" AS tp
                        WHERE tp."Permission" IN (1, 2)
                            AND tp."RepositoryId" = r."Id"
                    )
                )
                LIMIT 1;
            """


            # Execute the query with parameters
            cursor.execute(query, (repository_name, user_id))

            # Fetch the result
            result = cursor.fetchone()

            return result is not None

        except Exception as e:
            raise e
        finally:
            if cursor:
                cursor.close()
            if connection:
                connection.close()        