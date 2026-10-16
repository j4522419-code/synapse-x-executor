"""Add MB_TOPMOST only to the inspected 2.5.4 purchase confirmation."""
import hashlib,struct
import pefile
SOURCE_SHA256='afb11cb5b927f15e0a19a6364f007379acffeeeaacd3d9aaf29c43589380cc14'
TARGET_SHA256='2369a6ce65ae8510b394e46418b6b67b9e9a80586606d1d21e514b4df76aa5e8'
def apply_topmost(source):
    if hashlib.sha256(source).hexdigest()!=SOURCE_SHA256:raise ValueError('Unrecognized runtime; no purchase patch applied.')
    pe=pefile.PE(data=source);offset=pe.get_offset_from_rva(0x25fb10)
    if source[offset:offset+6]!=bytes.fromhex('41b931000100'):raise ValueError('Unexpected confirmation flags.')
    result=bytearray(source);result[offset:offset+6]=bytes.fromhex('41b931000500')
    checksum=pe.OPTIONAL_HEADER.get_field_absolute_offset('CheckSum')
    struct.pack_into('<I',result,checksum,pefile.PE(data=bytes(result)).generate_checksum())
    if hashlib.sha256(result).hexdigest()!=TARGET_SHA256:raise ValueError('Purchase patch verification failed.')
    return bytes(result)
