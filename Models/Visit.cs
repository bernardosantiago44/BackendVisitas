namespace BackendVisitas.Models
{
    // Registrar y consultar (administrar) visitas de empleados a clientes:
    // Panel de visitas a tal cliente de parte de X empleado.
    public class Visit
    {
        public int Id { get; set; }
        public int EmployeeID { get; set; }
        // Employee visits customer
        public int CustomerID { get; set; }
        public DateTime VisitDate { get; set; }

    }
}
