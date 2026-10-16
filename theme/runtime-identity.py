"""Patch the inspected runtime's identification metadata only."""
import struct

NAME='Synapse X'
VERSION='1.3.1-grand'
IDENTIFY_RVA=0x2439A0
IDENTIFY_END=0x243A97
TAIL_RVA=0x243A3C
PUSH_STRING_RVA=0x158020
STRLEN_RVA=0x38D49C

def patch_identity(data,pe):
    changes=[]
    def replace(rva,after,purpose):
        off=pe.get_offset_from_rva(rva);before=bytes(data[off:off+len(after)])
        data[off:off+len(after)]=after
        changes.append({'rva':hex(rva),'before':before.hex(),'after':after.hex(),'purpose':purpose})
    name=NAME.encode().ljust(16,b'\0');assert len(name)==16
    old=[]
    for index,(rva,key_rva) in enumerate([(0x2439AD,0x2439DF),(0x2439C6,0x2439F8)]):
        off=pe.get_offset_from_rva(rva);key_off=pe.get_offset_from_rva(key_rva)
        assert data[off:off+2]==data[key_off:key_off+2]==bytes.fromhex('48b8')
        encoded=struct.unpack_from('<Q',data,off+2)[0];key=struct.unpack_from('<Q',data,key_off+2)[0]
        old.append(struct.pack('<Q',encoded^key))
        replacement=bytes(a^b for a,b in zip(name[index*8:index*8+8],struct.pack('<Q',key)))
        replace(rva+2,replacement,'Identification name constant')
    assert b''.join(old).split(b'\0')[0]==b'Potassium'
    # Both public names are registered against this same function. Keep the
    # registration table intact; the first string push is also unchanged.
    ref=pe.get_offset_from_rva(0x69C52)
    assert data[ref:ref+3]==bytes.fromhex('488d1d')
    assert 0x69C59+struct.unpack_from('<i',data,ref+3)[0]==IDENTIFY_RVA
    off=pe.get_offset_from_rva(TAIL_RVA)
    assert data[off:off+8]==bytes.fromhex('48833dac1b25000f')
    # Reuse the existing 16-byte stack buffer, original Lua string-push helper,
    # stack frame and epilogue. Do not change the shared backend version global.
    version=VERSION.encode();assert len(version)<16
    padded=version.ljust(16,b'\0')
    tail=bytearray(bytes.fromhex('48b8')+padded[:8]+bytes.fromhex('4889442430'))
    tail+=bytes.fromhex('48b8')+padded[8:]+bytes.fromhex('4889442438')
    tail+=bytes.fromhex('488d54243041b8')+struct.pack('<I',len(version))+bytes.fromhex('488bcf')
    tail+=b'\xe8'+struct.pack('<i',PUSH_STRING_RVA-(TAIL_RVA+len(tail)+5))
    tail+=bytes.fromhex('b802000000488b5c24684883c4505fc3')
    assert len(tail)<=IDENTIFY_END-TAIL_RVA
    tail+=b'\x90'*(IDENTIFY_END-TAIL_RVA-len(tail))
    replace(TAIL_RVA,tail,'Identification version return, preserving the existing Lua API helper and function epilogue')
    return changes
