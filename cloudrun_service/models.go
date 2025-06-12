package main

// Apparatus represents the apparatus data structure
type Apparatus struct {
	Name                string           `json:"name"`
	Desc                string           `json:"desc"`
	ApparatusClass      string           `json:"apparatusClass"`
	Fields              []ApparatusField `json:"fields"`
	PossibleAttachments []string         `json:"possibleAttachments"`
}

type ApparatusField struct {
	Key         string   `json:"key"`
	Type        string   `json:"type"`
	RangeMin    *float64 `json:"rangeMin,omitempty"`
	RangeMax    *float64 `json:"rangeMax,omitempty"`
	IntegerOnly *bool    `json:"integerOnly,omitempty"`
	Options     []string `json:"options,omitempty"`
	Placeholder *string  `json:"placeholder,omitempty"`
}

// Experiment represents the experiment data structure
type Experiment struct {
	Name                    string                  `json:"name"`
	Subject                 string                  `json:"subject"`
	Topic                   string                  `json:"topic"`
	Class                   int                     `json:"class"`
	Theory                  string                  `json:"theory"`
	Procedure               string                  `json:"procedure"`
	AvailableVisualizations []string                `json:"availableVisualizations"`
	RequiredApparatus       []string                `json:"requiredApparatus"`
	InstantiatedApparatus   []InstantiatedApparatus `json:"instantiatedApparatus,omitempty"`
}

type InstantiatedApparatus struct {
	Name        string `json:"name"`
	ApparatusId int    `json:"apparatusId"`
}

// Visualization represents the visualization data structure
type Visualization struct {
	Name string `json:"name"`
	Desc string `json:"desc"`
}
