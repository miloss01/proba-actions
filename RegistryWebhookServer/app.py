
import os
from flask import Flask, jsonify, request
from datetime import datetime
import psycopg2
import uuid



# Load configuration from environment variables with defaults
WEBHOOK_HOST = os.environ.get("UKS_WEBHOOK_HOST", "0.0.0.0")
WEBHOOK_PORT = int(os.environ.get("UKS_WEBHOOK_PORT", 5002))
WEBHOOK_DEBUG = os.environ.get("UKS_WEBHOOK_DEBUG", "True").lower() in ("true", "1", "yes")
DATABASE_URI = os.environ.get("UKS_DATABASE_URI", "postgresql://admin:admin@localhost:5432/uks-database")

app = Flask(__name__)
app.debug = WEBHOOK_DEBUG

def parse_timestamp(timestamp_str):
    cleaned_timestamp = timestamp_str.rstrip("Z")
    return datetime.fromisoformat(cleaned_timestamp)


def update_database(tag, repository, timestamp, digest):
    connection = None
    cursor = None
    try:
        connection = psycopg2.connect(DATABASE_URI)
        cursor = connection.cursor()

        # Fetch repository ID
        query = """
            SELECT "Id" FROM "DockerRepositories" WHERE "Name" = %s;
        """
        cursor.execute(query, (repository,))
        repository_id = cursor.fetchone()
        repository_id = repository_id[0] if repository_id else None
        if not repository_id:
            return  # Return if repository doesn't exist

        # Delete related ImageTags
        query = """
            DELETE FROM "ImageTags"
            WHERE "Name" = %s AND "DockerImageId" IN (
                SELECT "Id" FROM "DockerImages" WHERE "DockerRepositoryId" = %s
            );
        """
        cursor.execute(query, (tag, repository_id))

        # Check if DockerImage already exists by Digest
        query = """
            SELECT "Id" FROM "DockerImages" WHERE "Digest" = %s;
        """
        cursor.execute(query, (digest,))
        image_id = cursor.fetchone()
        image_id = image_id[0] if image_id else None

        # Insert DockerImage if not found
        if not image_id:
            image_id = str(uuid.uuid4())
            query = """
                INSERT INTO "DockerImages" ("Id", "DockerRepositoryId", "LastPush", "Digest", "CreatedAt", "IsDeleted")
                VALUES (%s, %s, %s, %s, NOW(), false);
            """
            cursor.execute(query, (image_id, repository_id, timestamp, digest))
        
        # Insert ImageTag for the DockerImage
        tag_id = str(uuid.uuid4())
        query = """
            INSERT INTO "ImageTags" ("Id", "DockerImageId", "Name", "CreatedAt", "IsDeleted")
            VALUES (%s, %s, %s, NOW(), false);
        """
        cursor.execute(query, (tag_id, image_id, tag))
        connection.commit()

    except Exception as e:
        print(f"Error: {e}")
        connection.rollback()
    finally:
        if cursor:
            cursor.close()
        if connection:
            connection.close()

def parse_push_event(event):
    if not event.get("target").get("mediaType") == "application/vnd.docker.distribution.manifest.v2+json":
        return
    
    timestamp = parse_timestamp(event.get("timestamp"))
    repository_name = event.get("target").get("repository")
    manifest_digest = event.get("target").get("digest")
    tag = event.get("target").get("tag")

    update_database(tag, repository_name, timestamp, manifest_digest)

@app.route("/notify/push", methods=["POST"])
def handle_push_command():
    print(f"Method: {request.method}")
    print(f"URL: {request.url}")
    print(f"Headers: {request.headers}")
    
    if request.data:
        print(f"Body: {request.data.decode('utf-8')}")
    else:
        print("Body: None")

    data = request.get_json()

    for event in data.get("events"):
        if(event.get("action") == "push"):
            parse_push_event(event)
    

    return jsonify({"status": "received"}), 200

if __name__ == "__main__":
    app.run(host=WEBHOOK_HOST, port=WEBHOOK_PORT, debug=WEBHOOK_DEBUG)