package org.sample.contactservice.entity;

import java.io.Serializable;
import java.util.HashMap;
import java.util.Map;

import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.mapping.Document;

import io.swagger.annotations.ApiModelProperty;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Document
@Data
@AllArgsConstructor
@NoArgsConstructor
public class Person implements Serializable {
	
	private static final long serialVersionUID = 49994416335608157L;

	@Id
	@ApiModelProperty(name="ID", example="1060f9e4-be45-4516-bb71-5cd97c304254", 
	notes="Unique MongoDB identifier")
	private String id;
	
	@ApiModelProperty(name="First Name", example="John",
	notes="First Name of the Person")
	private String firstName;
	
	@ApiModelProperty(name="Last Name", example="Doe",
			notes="Last Name of the Person")
	private String lastName;
	
	@ApiModelProperty(name="Street Address", example="Street",
			notes="Street name w/o house number")
	private String streetAddress;
	
	@ApiModelProperty(name="House Number", example="42",
			notes="House Number")
	private String houseNumber;
	
	@ApiModelProperty(name="ZIP", example="10115",
			notes="House Number")
	private String zip;
	
	@ApiModelProperty(name="City", example="Berlin",
			notes="City Name")
	private String city;
	
	@ApiModelProperty(name="Country", example="Germany",
			notes="Name of the Country")
	private String country;
	
	//extension Fields
	@ApiModelProperty(name="Extension Fields",
			notes="Arbitrary json key value pairs")
	private Map<String, Object> extensionFields = new HashMap<String, Object>();

}
