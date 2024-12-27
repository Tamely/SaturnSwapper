export module Saturn.Toc.IoContainerSettings;

import Saturn.Structs.IoContainerId;
import Saturn.Structs.Guid;
import Saturn.Encryption.AES;
import Saturn.Structs.IoContainerFlags;

export struct FIoContainerSettings {
    FIoContainerId ContainerId;
    EIoContainerFlags ContainerFlags = EIoContainerFlags::None;
    FGuid EncryptionKeyGuid;
    FAESKey EncryptionKey;
    bool bGenerateDiffPatch = false;

    bool IsCompressed() const {
        return !!(ContainerFlags & EIoContainerFlags::Compressed);
    }

    bool IsEncrypted() const {
        return !!(ContainerFlags & EIoContainerFlags::Encrypted);
    }

    bool IsSigned() const {
        return !!(ContainerFlags & EIoContainerFlags::Signed);
    }

    bool IsIndexed() const {
        return !!(ContainerFlags & EIoContainerFlags::Indexed);
    }

    bool IsOnDemand() const {
        return !!(ContainerFlags & EIoContainerFlags::OnDemand);
    }
};