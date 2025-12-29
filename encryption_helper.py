import os
from cryptography.hazmat.primitives.kdf.pbkdf2 import PBKDF2HMAC
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend
from cryptography.hazmat.primitives import padding

class EncryptionHelper:
    # Matching your C# Salt exactly
    # NOTE: In a real production app, salt should be random per file, but we are matching your existing logic.
    _SALT = b"Y7W0xXp_o8zM5gR9-KqN-u2JtV1jA6bDcSfL3eH4cEwI7hG_rZlP8yFv4sQ_aT0bU9d"
    _KEY_SIZE = 32  # 256 bits = 32 bytes
    _ITERATIONS = 10000

    @staticmethod
    def _derive_key(password: str, salt: bytes) -> bytes:
        """Derives a 256-bit key using PBKDF2HMAC-SHA256, matching C# Rfc2898DeriveBytes."""
        kdf = PBKDF2HMAC(
            algorithm=hashes.SHA256(),
            length=EncryptionHelper._KEY_SIZE,
            salt=salt,
            iterations=EncryptionHelper._ITERATIONS,
            backend=default_backend()
        )
        return kdf.derive(password.encode('utf-8'))

    @staticmethod
    def encrypt_and_save(password: str, data_to_encrypt: str, file_path: str):
        """
        Encrypts string data and saves to file.
        Structure: [IV (16 bytes)] + [Encrypted Data]
        """
        key = EncryptionHelper._derive_key(password, EncryptionHelper._SALT)

        # Generate IV (Initialization Vector)
        iv = os.urandom(16)

        # Setup AES in CBC mode (Standard for C# default)
        cipher = Cipher(algorithms.AES(key), modes.CBC(iv), backend=default_backend())
        encryptor = cipher.encryptor()

        # Add PKCS7 Padding (Required because AES blocks must be 128-bit/16-byte aligned)
        padder = padding.PKCS7(128).padder()
        data_bytes = data_to_encrypt.encode('utf-8')
        padded_data = padder.update(data_bytes) + padder.finalize()

        encrypted_content = encryptor.update(padded_data) + encryptor.finalize()

        # Write to file: IV followed by Encrypted Data
        with open(file_path, 'wb') as f:
            f.write(iv)
            f.write(encrypted_content)

    @staticmethod
    def decrypt_and_load(password: str, file_path: str) -> str | None:
        """
        Reads file, extracts IV, decrypts, and returns string.
        Returns None on failure.
        """
        if not os.path.exists(file_path):
            return None

        try:
            with open(file_path, 'rb') as f:
                file_content = f.read()

            # Extract IV (first 16 bytes) and the actual encrypted payload
            iv = file_content[:16]
            encrypted_data = file_content[16:]

            key = EncryptionHelper._derive_key(password, EncryptionHelper._SALT)

            cipher = Cipher(algorithms.AES(key), modes.CBC(iv), backend=default_backend())
            decryptor = cipher.decryptor()

            # Decrypt
            padded_data = decryptor.update(encrypted_data) + decryptor.finalize()

            # Remove PKCS7 Padding
            unpadder = padding.PKCS7(128).unpadder()
            data = unpadder.update(padded_data) + unpadder.finalize()

            return data.decode('utf-8')

        except Exception as ex:
            # In C# you used Debug.WriteLine; in Python we usually just print or log
            print(f"Decryption Error: {ex}")
            return None

# --- Test Block (Run this file directly to verify) ---
if __name__ == "__main__":
    test_file = "test_encrypt.dat"
    test_pass = "MySecretPass"
    test_data = "Hello World! This is a test."

    print("1. Encrypting...")
    EncryptionHelper.encrypt_and_save(test_pass, test_data, test_file)
    
    print("2. Decrypting...")
    result = EncryptionHelper.decrypt_and_load(test_pass, test_file)
    
    print(f"Result: {result}")
    
    if result == test_data:
        print("SUCCESS: Data matches!")
    else:
        print("FAILURE: Data mismatch.")