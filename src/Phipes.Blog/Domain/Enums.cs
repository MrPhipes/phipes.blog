namespace Phipes.Blog.Domain;

/// <summary>Estado del ciclo de vida de una pieza de contenido (post o proyecto).</summary>
public enum ContentStatus
{
    /// <summary>Borrador: solo visible para su autor y administradores.</summary>
    Draft = 0,

    /// <summary>Enviado por un autor externo y a la espera de moderación.</summary>
    PendingReview = 1,

    /// <summary>Publicado y visible al público.</summary>
    Published = 2,

    /// <summary>Retirado de circulación sin borrarse.</summary>
    Archived = 3,
}

/// <summary>Estado de moderación de un comentario.</summary>
public enum CommentStatus
{
    Pending = 0,
    Approved = 1,
    Spam = 2,
    Rejected = 3,
}

/// <summary>Tipo de hito en la biografía/línea de tiempo profesional.</summary>
public enum BioEntryKind
{
    /// <summary>Experiencia laboral.</summary>
    Experience = 0,

    /// <summary>Formación académica.</summary>
    Education = 1,

    /// <summary>Certificación o credencial.</summary>
    Certification = 2,

    /// <summary>Hito o logro destacado.</summary>
    Milestone = 3,
}

/// <summary>Estado de un mensaje del formulario de contacto.</summary>
public enum ContactMessageStatus
{
    New = 0,
    Read = 1,
    Replied = 2,
    Archived = 3,
}
