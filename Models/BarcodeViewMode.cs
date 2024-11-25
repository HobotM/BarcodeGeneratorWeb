using System.ComponentModel.DataAnnotations;

public class BarcodeViewModel{

    [Required(ErrorMessage ="Please enter text to generate barcode.")]
    public string Text { get; set; }


    [Required(ErrorMessage ="Please select a barcode format.")]
    public string Format { get; set; }



}